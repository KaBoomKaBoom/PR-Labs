using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;


Console.Title = "WebSocket Chat Client";
Console.Clear();

var clientName = args.Length > 0 ? args[0] : "Client" + new Random().Next(1000);
var uri = new Uri("ws://localhost:5000/ws");
var cts = new CancellationTokenSource();

using var _webSocket = new ClientWebSocket();
var functions = new WebSocketFunctions(_webSocket, clientName);

try
{
    Console.WriteLine($"Connecting to chat server...");
    await _webSocket.ConnectAsync(uri, cts.Token);
    Console.Clear();
    Console.WriteLine("=== Welcome to the Chat Room ===\n");

    await functions.SendConnectMessageAsync(cts.Token);

    var receivingTask = functions.ReceiveMessagesAsync(cts.Token);

    // Allow some time for the connection to establish
    await Task.Delay(2000, cts.Token);

    // Send messages until we've sent the leave message
    while (_webSocket.State == WebSocketState.Open &&
           !cts.Token.IsCancellationRequested &&
           functions.ShouldContinue())
    {
        string message = functions.GetRandomMessage();
        await functions.SendMessageAsync(message, cts.Token);
        await Task.Delay(2000, cts.Token); // Adjusted delay to allow more interaction
    }

    // Send leave message before disconnecting
    await functions.SendLeaveMessageAsync(cts.Token); // Send the leave message

    cts.Cancel(); // Cancel after we're done sending messages
    await receivingTask;
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nDisconnected from chat.");
}
catch (WebSocketException ex)
{
    Console.WriteLine($"\nConnection error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"\nError: {ex.Message}");
}
finally
{
    if (_webSocket.State == WebSocketState.Open ||
        _webSocket.State == WebSocketState.CloseReceived)
    {
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
    }
    _webSocket.Dispose();
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}
