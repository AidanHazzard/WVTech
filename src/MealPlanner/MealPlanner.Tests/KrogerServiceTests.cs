using MealPlanner.Models.DTO;
using MealPlanner.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;

namespace MealPlanner.Tests;

public class KrogerServiceTests
{
    private static readonly string TokenJson =
        """{"access_token":"test-access-token","token_type":"bearer","expires_in":1800}""";

    private static (KrogerService service, Mock<HttpMessageHandler> handler) Setup(
        params (HttpStatusCode status, string json)[] responses)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        var seq = handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());

        foreach (var (status, json) in responses)
        {
            seq = seq.ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = status,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.kroger.com/v1/")
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Kroger:ClientId"] = "test-id",
                ["Kroger:ClientSecret"] = "test-secret",
                ["Kroger:RedirectUri"] = "https://localhost/Kroger/Callback"
            })
            .Build();

        return (new KrogerService(client, config), handler);
    }

    [Test]
    public void GetAuthorizationUrl_ContainsRequiredParams()
    {
        var (service, _) = Setup();

        var url = service.GetAuthorizationUrl("test-state");

        Assert.That(url, Does.Contain("response_type=code"));
        Assert.That(url, Does.Contain("client_id="));
        Assert.That(url, Does.Contain("redirect_uri="));
        Assert.That(url, Does.Contain("scope="));
        Assert.That(url, Does.Contain("state=test-state"));
    }

    [Test]
    public async Task FindNearestStoresAsync_ReturnsStores_WhenResponseOk()
    {
        var locationsJson = """
            {"data":[
                {"locationId":"70400340","name":"Kroger",
                 "address":{"addressLine1":"123 Main St","city":"Eugene","state":"OR","zipCode":"97401"}}
            ]}
            """;
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson),
            (HttpStatusCode.OK, locationsJson));

        var result = await service.FindNearestStoresAsync("97401");

        Assert.That(result, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].LocationId, Is.EqualTo("70400340"));
            Assert.That(result[0].Name, Is.EqualTo("Kroger"));
            Assert.That(result[0].City, Is.EqualTo("Eugene"));
            Assert.That(result[0].ZipCode, Is.EqualTo("97401"));
        }
    }

    [Test]
    public async Task FindNearestStoresAsync_ReturnsEmpty_WhenNoLocations()
    {
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson),
            (HttpStatusCode.OK, """{"data":[]}"""));

        var result = await service.FindNearestStoresAsync("00000");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task FindNearestStoresAsync_ReturnsEmpty_WhenTokenFails()
    {
        var (service, _) = Setup(
            (HttpStatusCode.Unauthorized, ""));

        var result = await service.FindNearestStoresAsync("97401");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task FindNearestStoresAsync_ReturnsEmpty_WhenLocationsFail()
    {
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson),
            (HttpStatusCode.InternalServerError, ""));

        var result = await service.FindNearestStoresAsync("97401");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task FindNearestStoresAsync_ReturnsMultipleStores_WhenResponseHasMany()
    {
        var locationsJson = """
            {"data":[
                {"locationId":"111","name":"Kroger","address":{"addressLine1":"1 A St","city":"Eugene","state":"OR","zipCode":"97401"}},
                {"locationId":"222","name":"Fred Meyer","address":{"addressLine1":"2 B St","city":"Springfield","state":"OR","zipCode":"97477"}}
            ]}
            """;
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson),
            (HttpStatusCode.OK, locationsJson));

        var result = await service.FindNearestStoresAsync("97401");

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[1].LocationId, Is.EqualTo("222"));
    }

    [Test]
    public async Task SearchProductUpcAsync_ReturnsUpc_WhenResponseOk()
    {
        var productsJson = """{"data":[{"upc":"0000000004072","description":"RUSSET POTATO","items":[{"size":"1 ct"}]}]}""";
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson),
            (HttpStatusCode.OK, productsJson));

        var result = await service.SearchProductUpcAsync("potato", "70400340", 1, "Count");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Upc, Is.EqualTo("0000000004072"));
    }

    [Test]
    public async Task SearchProductUpcAsync_CalculatesQuantityFromProductSize()
    {
        var productsJson = """{"data":[{"upc":"0001111042504","description":"Chicken Broth","items":[{"size":"32 oz"}]}]}""";
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson),
            (HttpStatusCode.OK, productsJson));

        // 5 cups = 40 oz, product is 32 oz → need 2 units
        var result = await service.SearchProductUpcAsync("chicken broth", "70400340", 5, "Cup(s)");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Quantity, Is.EqualTo(2));
    }

    [Test]
    public async Task SearchProductUpcAsync_CalculatesQuantity_WhenProductSizeUsesFullWordUnit()
    {
        var productsJson = """{"data":[{"upc":"0001111099999","description":"Water","items":[{"size":"1 gallon"}]}]}""";
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson),
            (HttpStatusCode.OK, productsJson));

        // 4.6 L = 155.74 oz, 1 gallon = 128 oz → need 2 units
        var result = await service.SearchProductUpcAsync("water", "70400340", 4.6f, "L");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Quantity, Is.EqualTo(2));
    }

    [Test]
    public async Task SearchProductUpcAsync_QuantityIsOne_WhenOuncesLessThanPackageSize()
    {
        var productsJson = """{"data":[{"upc":"0001111099001","description":"Kidney Beans","items":[{"size":"15.5 oz"}]}]}""";
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson),
            (HttpStatusCode.OK, productsJson));

        // 8 oz recipe, 15.5 oz can → need 1 unit
        var result = await service.SearchProductUpcAsync("kidney beans", "70400340", 8, "Ounce(s)");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Quantity, Is.EqualTo(1));
    }

    [Test]
    public async Task SearchProductUpcAsync_ReturnsNull_WhenNoProducts()
    {
        var emptyJson = """{"data":[]}""";
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson),
            (HttpStatusCode.OK, emptyJson));

        var result = await service.SearchProductUpcAsync("xyzunknown", "70400340", 1, "Count");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SearchProductUpcAsync_ReturnsNull_WhenApiFails()
    {
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson),
            (HttpStatusCode.BadRequest, ""));

        var result = await service.SearchProductUpcAsync("potato", "70400340", 1, "Count");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ExportCartAsync_ReturnsTrue_WhenResponseOk()
    {
        var (service, _) = Setup(
            (HttpStatusCode.NoContent, ""));

        var items = new[] { new KrogerCartItem { Upc = "0000000004072", Quantity = 2 } };
        var result = await service.ExportCartAsync(items, "user-access-token");

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExportCartAsync_ReturnsFalse_WhenResponseFails()
    {
        var (service, _) = Setup(
            (HttpStatusCode.Unauthorized, ""));

        var items = new[] { new KrogerCartItem { Upc = "0000000004072", Quantity = 1 } };
        var result = await service.ExportCartAsync(items, "expired-token");

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task ExchangeCodeAsync_ReturnsToken_WhenResponseOk()
    {
        var (service, _) = Setup(
            (HttpStatusCode.OK, TokenJson));

        var result = await service.ExchangeCodeAsync("auth-code-123");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AccessToken, Is.EqualTo("test-access-token"));
    }

    [Test]
    public async Task ExchangeCodeAsync_ReturnsNull_WhenResponseFails()
    {
        var (service, _) = Setup(
            (HttpStatusCode.BadRequest, ""));

        var result = await service.ExchangeCodeAsync("bad-code");

        Assert.That(result, Is.Null);
    }
}
