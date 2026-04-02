using MealPlanner.Models;
using MealPlanner.Models.DTO;
using MealPlanner.Services;
using System.Net;
using Moq;
using Moq.Protected;

namespace MealPlanner.Tests;

public class EdamamServiceTests
{
    private EdamamService SetupMocks(string jsonResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var messageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        messageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = statusCode,
                Content = new StringContent(jsonResponse)
            })
            .Verifiable();

        var client = new HttpClient(messageHandler.Object)
        {
            BaseAddress = new Uri("https://api.test.com/api/")
        };

        return new EdamamService(client, "testid", "testkey");
    }

    [Test]
    public async Task SearchExternalRecipesByName_Returns1Recipe_IfResponseOK()
    {
        // Arrange
        string json = 
            """
            {
                "from": 1,
                "to": 1,
                "count": 1,
                "_links": {},
                "hits": [
                    {
                        "recipe": {
                            "uri": "http://TestUri.com/Test123",
                            "label": "Chocolate Chip Oatmeal Cookies"
                        },
                        "_links": {
                            "self": {
                                "href": "https://api.test.com/api/test123",
                                "title": "Self"
                            }
                        }
                    }
                ]
            }
            """;

        EdamamService service = SetupMocks(json);

        // Act
        var result = await service.SearchExternalRecipesByName("oatmeal");

        // Assert
        Assert.That(result, Is.Not.Empty);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Chocolate Chip Oatmeal Cookies"));
            Assert.That(result.First().ExternalUri, Is.EqualTo("http://TestUri.com/Test123"));
        }
    }

    [Test]
    public async Task SearchExternalRecipesByName_ReturnsManyRecipes_IfResponseOK()
    {
        // Arrange
        string json = 
            """
            {
                "from": 1,
                "to": 4,
                "count": 4,
                "_links": {},
                "hits": [
                    {
                        "recipe": {
                            "uri": "http://TestUri.com/Test1",
                            "label": "Spaghetti1"
                        },
                        "_links": {
                            "self": {
                                "href": "https://api.test.com/api/test123",
                                "title": "Self"
                            }
                        }
                    },
                    {
                        "recipe": {
                            "uri": "http://TestUri.com/Test12",
                            "label": "Spaghetti2"
                        },
                        "_links": {
                            "self": {
                                "href": "https://api.test.com/api/test123",
                                "title": "Self"
                            }
                        }
                    },
                    {
                        "recipe": {
                            "uri": "http://TestUri.com/Test123",
                            "label": "Spaghetti3"
                        },
                        "_links": {
                            "self": {
                                "href": "https://api.test.com/api/test123",
                                "title": "Self"
                            }
                        }
                    },
                    {
                        "recipe": {
                            "uri": "http://TestUri.com/Test1234",
                            "label": "Spaghetti4"
                        },
                        "_links": {
                            "self": {
                                "href": "https://api.test.com/api/test123",
                                "title": "Self"
                            }
                        }
                    }
                ]
            }
            """;

        EdamamService service = SetupMocks(json);

        // Act
        var result = await service.SearchExternalRecipesByName("spaghetti");

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Count, Is.EqualTo(4));
    }

    [Test]
    public async Task SearchExternalRecipesByName_ReturnsNoRecipes_IfResponseOK()
    {
        // Arrange
        string json = 
            """
            {
                "from": 0,
                "to": 0,
                "count": 0,
                "_links": {},
                "hits": []
            }
            """;

        EdamamService service = SetupMocks(json);

        // Act
        var result = await service.SearchExternalRecipesByName("no recipes");

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task SearchExternalRecipesByName_ThrowsError_IfResponseNotOK()
    {
        // Arrange
        EdamamService service = SetupMocks("", HttpStatusCode.Forbidden);
        // Act
        var exception = Assert.CatchAsync(
            async () => await service.SearchExternalRecipesByName("oatmeal"));
        
        // Assert
        Assert.That(exception, Is.TypeOf<Exception>());
    }

    [Test]
    public async Task GetExternalRecipeByURI_ReturnsRecipe_IfResponseOK()
    {
        // Arrange
        string json = 
            """
            {
                "from": 1,
                "to": 1,
                "count": 1,
                "_links": {},
                "hits": [
                    {
                        "recipe": {
                            "uri": "http://TestUri.com/Test123",
                            "label": "Chocolate Chip Oatmeal Cookies",
                            "ingredients": [
                                {
                                    "text": "1/2 cup Test",
                                    "quantity": 0.5,
                                    "measure": "cup",
                                    "food": "test",
                                    "weight": 1.1,
                                    "foodCategory": "test cat",
                                    "foodId": "food_1234",
                                    "image": "https://www.test.com/img/1"
                                },
                                {
                                    "text": "3 teaspoon Test2",
                                    "quantity": 3,
                                    "measure": "teaspoon",
                                    "food": "test2",
                                    "weight": 1.1,
                                    "foodCategory": "test2 cat",
                                    "foodId": "food_1234",
                                    "image": "https://www.test.com/img/2"
                                },
                                {
                                    "text": "5 1/2 teaspoon Test3",
                                    "quantity": 5.5,
                                    "measure": "teaspoon",
                                    "food": "test2",
                                    "weight": 1.1,
                                    "foodCategory": "test2 cat",
                                    "foodId": "food_1234",
                                    "image": "https://www.test.com/img/2"
                                }
                            ],
                            "totalNutrients": {
                                "ENERC_KCAL": {
                                    "label": "Energy",
                                    "quantity": 1,
                                    "unit": "kcal"
                                },
                                "FAT": {
                                    "label": "Fat",
                                    "quantity": 2,
                                    "unit": "g"
                                },
                                "CHOCDF": {
                                    "label": "Carbs",
                                    "quantity": 3,
                                    "unit": "g"
                                },
                                "CHOCDF.net": {
                                    "label": "Carbohydrates (net)",
                                    "quantity": 4,
                                    "unit": "g"
                                },
                                "PROCNT": {
                                    "label": "Protein",
                                    "quantity": 5,
                                    "unit": "g"
                                }
                            }
                        },
                        "_links": {
                            "self": {
                                "href": "https://api.test.com/api/test123",
                                "title": "Self"
                            }
                        }
                    }
                ]
            }
            """;
        EdamamService service = SetupMocks(json);
        // Act
        var result = await service.GetExternalRecipeByURI("http://TestUri.com/Test123");

        // Assert
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Name, Is.EqualTo("Chocolate Chip Oatmeal Cookies"));
            Assert.That(result.ExternalUri, Is.EqualTo("http://TestUri.com/Test123"));
            Assert.That(result.Calories, Is.EqualTo(1));
            Assert.That(result.Fat, Is.EqualTo(2));
            Assert.That(result.Carbs, Is.EqualTo(3));
            Assert.That(result.Protein, Is.EqualTo(5));
            Assert.That(result.Ingredients.Count, Is.EqualTo(3));
            var firstIngredient = result.Ingredients.First();
            Assert.That(firstIngredient.Amount, Is.EqualTo(0.5).Within(0.005));
            Assert.That(firstIngredient.Measurement.Name, Is.EqualTo("cup"));
            Assert.That(firstIngredient.IngredientBase.Name, Is.EqualTo("test"));
        }
        
    }

    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.NotFound)]
    public async Task GetExternalRecipeByURI_ThrowsError_IfResponseNotOk(HttpStatusCode statusCode)
    {
        // Arrange
        EdamamService service = SetupMocks("", statusCode);

        // Act
        var exception = Assert.CatchAsync(
            async () => await service.GetExternalRecipeByURI("http://TestUri.com/Test123"));
        
        // Assert
        Assert.That(exception, Is.TypeOf<Exception>());
    }
}