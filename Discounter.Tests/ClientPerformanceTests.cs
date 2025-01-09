using Discounter.Core;
using Microsoft.AspNetCore.SignalR.Client;

namespace Discounter.Tests;

public class ClientPerformanceTests {
    [Test(Description = "ensure the server is up before running the test")]
    public async Task ShouldGenerateTwoMillionCodes_UnderTenSeconds() {
        var connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:5001/discountHub", options => {
                options.HttpMessageHandlerFactory = _ => new SocketsHttpHandler {
                    EnableMultipleHttp2Connections = true
                };
            })
            .WithAutomaticReconnect()
            .Build();

        await connection.StartAsync();

        List<Task> tasks = new();
        var request = new Models.GenerateRequest(2000, 8);
        for (var i = 0; i < 1000; i++) {
            tasks.Add(Task.Run(async () => {
                var generatedCodes = connection.StreamAsync<string>("GenerateCodes", request);
                try {
                    await foreach (var _ in generatedCodes) { }
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    throw;
                }
            }));
        }
        
        await Task.WhenAll(tasks);
    }
}