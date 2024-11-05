using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Lab2.Helpers;

namespace Lab2.Services
{
    public class ChatRoom
    {
        private ConcurrentDictionary<WebSocket, string> _sockets = new();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public void AddConnection(WebSocket socket)
        {
            _sockets[socket] = null; // Initially set as null
            _ = ReceiveMessages(socket);
        }

        private async Task ReceiveMessages(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await HandleClientDisconnection(socket);
                        break;
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var chatMessage = ChatMessage.Deserialize(jsonMessage);

                        // Check if this is a connect message and set the name if not already set
                        if (chatMessage.Type == "connect" && _sockets[socket] == null)
                        {
                            _sockets[socket] = chatMessage.Name;
                            await BroadcastMessage($"{chatMessage.Name} connected to {chatMessage.Room}");
                        }
                        else if (chatMessage.Type == "message")
                        {
                            await BroadcastMessage($"{chatMessage.Name}: {chatMessage.Content}");
                        }
                    }
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
                await HandleClientDisconnection(socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                await HandleClientDisconnection(socket);
            }
        }

        private async Task HandleClientDisconnection(WebSocket socket)
        {
            if (_sockets.TryRemove(socket, out string clientName))
            {
                await BroadcastMessage($"{clientName} has left the chat.");
            }

            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Client disconnected",
                        CancellationToken.None
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during disconnection: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Client disconnected and removed from room.");
            }
        }

        private async Task BroadcastMessage(string message)
        {
            var deadSockets = new List<WebSocket>();
            var messageBuffer = Encoding.UTF8.GetBytes(message);

            foreach (var socket in _sockets.Keys)
            {
                if (socket.State != WebSocketState.Open)
                {
                    deadSockets.Add(socket);
                    continue;
                }

                try
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(messageBuffer),
                        WebSocketMessageType.Text,
                        true,
                        _cts.Token
                    );
                }
                catch (Exception)
                {
                    deadSockets.Add(socket);
                }
            }

            foreach (var deadSocket in deadSockets)
            {
                await HandleClientDisconnection(deadSocket);
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

}
