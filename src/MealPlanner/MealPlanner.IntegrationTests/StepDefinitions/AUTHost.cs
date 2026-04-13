using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using MealPlanner.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.IntegrationTests;

public sealed class AUTHost : IDisposable
{
    private readonly string _baseUrl;
    private readonly Process _process;
    private bool _disposed;
    private static string _connectionString;

    private AUTHost()
    {
        string projectRoot = GetProjectRoot();
        int port = GetOpenPort();
        _baseUrl = $"http://localhost:{port}";
        _connectionString = Environment.GetEnvironmentVariable("ConnectionString")
            ?? "Data Source=localhost,1433;Database=MealPlannerDb;User ID=sa;Password=MealPlanner!1234;Pooling=False;Trust Server Certificate=True;Authentication=SqlPassword";

        ProcessStartInfo startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = projectRoot,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(projectRoot);
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(_baseUrl);
        startInfo.ArgumentList.Add("-e");
        startInfo.ArgumentList.Add("ASPNETCORE_ENVIRONMENT=Staging");
        startInfo.ArgumentList.Add("--ConnectionStrings:DefaultConnection");
        startInfo.ArgumentList.Add(_connectionString);
        
        _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

        _process.Start();
        
        WaitForServerReady().GetAwaiter().GetResult();
    }

    public static string BaseUrl => Instance.Value._baseUrl;
    private static readonly Lazy<AUTHost> Instance = new(() => new AUTHost());
    public static void Start(string connection)
    {
        _connectionString = connection;
        _ = Instance.Value;
    } 
    public static void Stop()
    {
        if (Instance.IsValueCreated) Instance.Value.Dispose();
    }

    private static string GetProjectRoot()
    {
        return Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "MealPlanner"));
    }

    private static int GetOpenPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private async Task WaitForServerReady()
    {
        using HttpClient client = new HttpClient();
        DateTime timeoutAt = DateTime.UtcNow.AddSeconds(20);

        while (DateTime.UtcNow < timeoutAt)
        {
            if(_process.HasExited)
            {
                throw new InvalidOperationException(
                    "Application exited before ready"
                );
            }

            try
            {
                using var response = await client.GetAsync($"{_baseUrl}");
                if (response.IsSuccessStatusCode) 
                {
                    Console.WriteLine("Server ready for testing");
                    return;
                }
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("Server still starting...");
            }

            await Task.Delay(200);
        }

        throw new TimeoutException(
            $"Timed out while awaiting to contact {_baseUrl}"
        );
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
            _process.WaitForExit();
        }

        _process.Dispose();
    }

}