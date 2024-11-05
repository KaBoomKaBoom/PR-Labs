using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Lab2.Helpers;

namespace Lab2.Services
{
    public class ChatRoom
    {
        private ConcurrentDictionary<WebSocket, string> _sockets = new();

        //cancelattion token (connection)
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public void AddConnection(WebSocket socket)
        {
            _sockets[socket] = string.Empty;
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

                        if (chatMessage.Type == "connect")
                        {
                            Console.WriteLine($"{chatMessage.Name} connected to {chatMessage.Room}");
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
            if (_sockets.TryRemove(socket, out string clientName)) // Try to remove the socket and get the client name
            {
                await BroadcastMessage($"{clientName} has left the chat."); // Broadcast leave message
            }

            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    // close socket
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

            // Encode message
            var messageBuffer = Encoding.UTF8.GetBytes(message);

            foreach (var socket in _sockets.Keys)
            {
                // Check if socket is still open
                if (socket.State != WebSocketState.Open)
                {
                    deadSockets.Add(socket);
                    continue;
                }

                try
                {
                    // Send message to socket
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

            // Handle disconnections for dead sockets
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
