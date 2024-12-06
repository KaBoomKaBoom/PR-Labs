using Lab2.Helpers;
using Lab2.Models;
using Lab2.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// Add the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddCors((options) =>
    {
        options.AddPolicy("DevCors", (corsBuilder) =>
            {
                corsBuilder.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        options.AddPolicy("ProdCors", (corsBuilder) =>
            {
                corsBuilder.WithOrigins("https://myProductionSite.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
    });
// Register DbContext with SQL Server
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<DbCompletition>();

// Raft Node setup
var nodeId = Environment.GetEnvironmentVariable("NODE_ID") ?? "node1";
var currentPort = int.Parse(Environment.GetEnvironmentVariable("NODE_PORT") ?? "8080"); // Use internal port

var peersString = Environment.GetEnvironmentVariable("PEERS") ?? "";
var peers = peersString.Split(',')
    .Select(peer => 
    {
        var parts = peer.Split(':');
        return $"127.0.0.1:{parts[1]}";
    })
    .Where(peer => !peer.Contains($"127.0.0.1:{currentPort}"))
    .ToList();

// Initialize Raft Node
var raftNode = new RaftNode(currentPort, nodeId, peers);
raftNode.Start();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Lifetime.ApplicationStopping.Register(() =>
{
    // Stop Raft node on application shutdown
    raftNode.Stop();
});

// Optionally, setup WebSocket chat room (if applicable)
/// var webSocketRoom = new ChatRoom();
// var webSocketServerService = new WebSocketServerService();
// Task.Run(() => webSocketServerService.StartWebSocketServer(webSocketRoom));

app.Run();
