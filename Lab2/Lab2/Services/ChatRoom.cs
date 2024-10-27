using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Lab2.Services
{
    public class ChatRoom
    {
        private ConcurrentDictionary<WebSocket, string> _sockets = new();
        
        public void AddConnection(WebSocket socket)
        {
            _sockets[socket] = string.Empty;
            ReceiveMessages(socket);
        }

        private async void ReceiveMessages(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];

            while(socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if(result.MessageType == WebSocketMessageType.Close)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    _sockets.TryRemove(socket, out _);
                    break;
                }
                else if(result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    BroadcastMessage(message);
                }
            }
        }

        private async void BroadcastMessage(string message)
        {
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var tasks = new List<Task>();

            foreach(var socket in _sockets.Keys)
            {
                if(socket.State == WebSocketState.Open)
                {
                    tasks.Add(socket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }

            await Task.WhenAll(tasks);
        }


    }
}