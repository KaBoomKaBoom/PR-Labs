using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

public class ChatMessage
{
    public string? Type { get; set; }
    public string? Name { get; set; }
    public string? Room { get; set; }
    public string? Content { get; set; }

    public static string Serialize(ChatMessage message)
    {
        return JsonSerializer.Serialize(message);
    }

    public static ChatMessage Deserialize(string json)
    {
        return JsonSerializer.Deserialize<ChatMessage>(json);
    }
}

public class WebSocketFunctions
{
    private readonly ClientWebSocket _webSocket;
    private static readonly Random _random = new Random();
    private readonly string _clientName;
    private static readonly object _consoleLock = new object();
    private int _messageCount = 0;
    private bool _hasJoined = false;
    private const int MessageBeforeLeaving = 5; // Number of random messages before leaving
    public string ClientName => _clientName; // Read-only property to access _clientName

    public WebSocketFunctions(ClientWebSocket webSocket, string clientName)
    {
        _webSocket = webSocket;
        _clientName = clientName;
    }

    public async Task SendConnectMessageAsync(CancellationToken cancellationToken)
    {
        var connectMessage = new ChatMessage
        {
            Type = "connect",
            Name = _clientName,
            Room = "default"
        };

        await SendMessageAsync(ChatMessage.Serialize(connectMessage), cancellationToken);
    }
    public string GetRandomMessage()
    {
        if (!_hasJoined)
        {
            _hasJoined = true;
            return ChatMessage.Serialize(new ChatMessage
            {
                Type = "connect",
                Name = _clientName,
                Room = "default"
            });
        }

        if (_messageCount >= MessageBeforeLeaving)
        {
            return null; // Return null when ready to leave
        }

        _messageCount++;
        string content = GenerateRandomString();
        return ChatMessage.Serialize(new ChatMessage
        {
            Type = "message",
            Name = _clientName,
            Room = "default",
            Content = content
        });
    }

    private string GenerateRandomString()
    {
        int length = _random.Next(5, 20);
        var message = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            char randomChar = (char)_random.Next(33, 127);
            message.Append(randomChar);
        }

        return message.ToString();
    }

    public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        if (_webSocket.State != WebSocketState.Open || message == null)
            return;

        var buffer = Encoding.UTF8.GetBytes(message);
        var segment = new ArraySegment<byte>(buffer);
        await _webSocket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
    }

    public async Task SendLeaveMessageAsync(CancellationToken cancellationToken)
    {
        if (_webSocket.State != WebSocketState.Open)
            return;

        string leaveMessage = ChatMessage.Serialize(new ChatMessage
        {
            Type = "leave",
            Name = _clientName,
            Room = "default"
        });
        await SendMessageAsync(leaveMessage, cancellationToken); // Send the leave message
    }

    public async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4];

        try
        {
            while (!cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    PrintMessage(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    PrintSystemMessage("Chat connection closed by server");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            PrintSystemMessage("Chat session ended");
        }
        catch (WebSocketException ex)
        {
            PrintSystemMessage($"Connection error: {ex.Message}");
            throw;
        }
    }

    private void PrintMessage(string message)
    {
        lock (_consoleLock)
        {
            Console.WriteLine(message);
        }
    }

    private void PrintSystemMessage(string message)
    {
        lock (_consoleLock)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[System] {message}");
            Console.ResetColor();
        }
    }

    public bool ShouldContinue()
    {
        return _messageCount < MessageBeforeLeaving || !_hasJoined;
    }
}

class Program
{
    static async Task Main(string[] args)
    {
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
                await Task.Delay(3000, cts.Token); // Adjusted delay to allow more interaction
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
    }
}
