using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using System.Threading.Tasks;

namespace Lab2.Services
{
    public class WebSocketServerService
    {
        public async Task StartWebSocketServer(ChatRoom chatRoom)
        {
            var builder = WebApplication.CreateBuilder();
            
            // Add WebSocket services
            builder.Services.AddWebSockets(options =>
            {
                //keep the connection alive 2 minutes
                options.KeepAliveInterval = TimeSpan.FromMinutes(2);

                //buffer size
                options.ReceiveBufferSize = 1024 * 4;
            });

            var webSocketApp = builder.Build();
            
            // Configure WebSocket middleware
            webSocketApp.UseWebSockets();

            webSocketApp.Map("/ws", async (HttpContext context) =>
            {
                //check if the request is a WebSocket
                if (context.WebSockets.IsWebSocketRequest)
                {
                    //accept the WebSocket connection
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    Console.WriteLine("New WebSocket connection established.");
                    
                    //add the connection to the chat room
                    chatRoom.AddConnection(webSocket);
                    
                    // Keep the connection alive until the socket is closed
                    var tcs = new TaskCompletionSource<object>();
                    context.RequestAborted.Register(() => tcs.TrySetResult(null));
                    await tcs.Task;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            });

            await webSocketApp.RunAsync("http://0.0.0.0:5000");
        }
    }
}