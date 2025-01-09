using Discounter.Core;
using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Starting Discount Code Client...");

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/discountHub", options => {
        options.HttpMessageHandlerFactory = _ => new SocketsHttpHandler() {
            EnableMultipleHttp2Connections = true
        };
    })
    .WithAutomaticReconnect()
    .Build();

connection.Reconnecting += error => {
    Console.WriteLine($"Connection lost due to error: {error?.Message}. Reconnecting...");
    return Task.CompletedTask;
};

connection.Reconnected += connectionId => {
    Console.WriteLine($"Reconnected. ConnectionId: {connectionId}");
    return Task.CompletedTask;
};

connection.Closed += error => {
    if (error?.Message != null) {
        Console.WriteLine($"Connection closed due to error: {error.Message}");   
    }
    return Task.CompletedTask;
};

connection.On<string>("OnError", (errorMessage) =>
{
    Console.WriteLine($"Error from server: {errorMessage}");
});

try {
    await connection.StartAsync();
    Console.WriteLine("Connected to the DiscountHub.");
}
catch (Exception e) {
    Console.WriteLine($"Error connecting to the hub: {e.ToString()}");
    return;
}

await StartRepl(connection);

static async Task StartRepl(HubConnection connection) {
    while (true) {
        Console.WriteLine("\nAvailable Commands:");
        Console.WriteLine("1. Generate Discount Codes");
        Console.WriteLine("2. Use a Discount Code");
        Console.WriteLine("3. Exit");
        Console.Write("Enter your choice: ");
        var choice = Console.ReadLine();

        switch (choice?.Trim()) {
            case "1":
                await HandleGenerateCodes(connection);
                break;
            case "2":
                await HandleUseCode(connection);
                break;
            case "3":
                Console.WriteLine("Exiting...");
                await connection.StopAsync();
                return;
            default:
                Console.WriteLine("Invalid choice. Please select 1, 2, or 3.");
                break;
        }
    }
}

static async Task HandleGenerateCodes(HubConnection connection) {
    ushort count;
    byte length;

    while (true) {
        Console.WriteLine("Enter the number of codes to generate: ");
        var countInput = Console.ReadLine();
        if (ushort.TryParse(countInput, out count)) break;
        Console.WriteLine("Invalid input. Please try again");
    }

    while (true) {
        Console.Write("Enter the lengh of the code: ");
        var lengthInput = Console.ReadLine();
        if (byte.TryParse(lengthInput, out length)) break;
        Console.WriteLine("Invalid input. Please try again.");
    }

    var request = new Models.GenerateRequest(count, length);

    Console.WriteLine("Generating discount codes...");

    try {
        var stream = connection.StreamAsync<string>("GenerateCodes", request);

        await foreach (var code in stream) {
            Console.WriteLine(code);
        }

        Console.WriteLine("Discount codes generation completed.");
    }
    catch (Exception e) {
        Console.WriteLine($"Error generating codes: {e}");
    }
}

static async Task HandleUseCode(HubConnection connection) {
    Console.Write("Enter the discount code to use: ");
    var code = Console.ReadLine();

    var request = new Models.UseCodeRequest(code!.Trim());

    Console.WriteLine("Using discount code...");

    try {
        var response = await connection.InvokeAsync<Models.UseCodeResponse>("UseCode", request);
        Console.WriteLine($"Result: {response.Result.ToString()}, Message: {response.Message}");
    }
    catch (Exception ex) {
        Console.WriteLine($"Error using code: {ex.Message}");
    }
}