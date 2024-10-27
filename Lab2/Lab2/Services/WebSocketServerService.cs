namespace Lab2.Services
{
    public class WebSocketServerService
    {

        public async Task StartWebSocketServer(ChatRoom chatRoom)
        {
            var webSocketApp = WebApplication.CreateBuilder().Build();
            webSocketApp.UseWebSockets();
            webSocketApp.Map("/ws", (HttpContext contex) =>
            {
                if (contex.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = contex.WebSockets.AcceptWebSocketAsync().Result;
                    chatRoom.AddConnection(webSocket);
                }
                else
                {
                    contex.Response.StatusCode = 400;
                }
            });
            await webSocketApp.RunAsync("http://localhost:8081");
        }
    }
}