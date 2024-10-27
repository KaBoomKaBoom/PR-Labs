using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketFunctions
{
    private static ClientWebSocket _webSocket;
    private static readonly string[] Messages = { "join_room", "send_msg", "leave_room" };
    private static Random _random = new Random();
    private readonly string _clientName;

    public WebSocketFunctions(ClientWebSocket webSocket, string clientName)
    {
        _webSocket = webSocket;
        _clientName = clientName;
    }

    public string GetRandomMessage()
    {
        int index = _random.Next(Messages.Length);
        return $"{_clientName}: {Messages[index]}";
    }

    public async Task SendMessageAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(buffer);
        await _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024 * 4];

        while (_webSocket.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received: {message}");
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var clientName = args.Length > 0 ? args[0] : "Client" + new Random().Next(1000);
        var uri = new Uri("ws://localhost:5000/ws");

        using var _webSocket = new ClientWebSocket();
        var functions = new WebSocketFunctions(_webSocket, clientName);

        try
        {
            await _webSocket.ConnectAsync(uri, CancellationToken.None);
            Console.WriteLine($"{clientName} connected to WebSocket server.");

            // Start receiving messages in a separate task
            var receivingTask = functions.ReceiveMessagesAsync();

            // Continuously send random messages every 5 seconds
            while (_webSocket.State == WebSocketState.Open)
            {
                string message = functions.GetRandomMessage();
                await functions.SendMessageAsync(message);
                Console.WriteLine($"Sent by {clientName}: {message}");

                // Wait for 5 seconds before sending the next message
                await Task.Delay(5000);
            }

            // Wait for the receiving task to complete before closing the socket
            await receivingTask;
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"{clientName} WebSocket error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{clientName} Error: {ex.Message}");
        }
        finally
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            Console.WriteLine($"{clientName} WebSocket connection closed.");
        }
    }
}
