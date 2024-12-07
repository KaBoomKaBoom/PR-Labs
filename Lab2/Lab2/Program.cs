using Lab2.Helpers;
using Lab2.Models;
using Lab2.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.InteropServices;

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

// Read environment variables to configure the node
string nodeId = Environment.GetEnvironmentVariable("NODE_ID") ?? "node1";
int nodePort = int.Parse(Environment.GetEnvironmentVariable("NODE_PORT") ?? "8081");
string peersEnv = Environment.GetEnvironmentVariable("PEERS") ?? string.Empty;
string[] peers = peersEnv.Split(',', StringSplitOptions.RemoveEmptyEntries);

var app = builder.Build();

// Create and start the Raft node
var node = new RaftNode(nodeId, nodePort, peers);
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

Console.WriteLine($"Starting node {nodeId} on port {nodePort}");

// Handle both SIGTERM (Docker) and CTRL+C
AppDomain.CurrentDomain.ProcessExit += (s, e) => 
{
    Console.WriteLine($"[{nodeId}] Received shutdown signal. Starting graceful shutdown...");
    node.Stop();
    Console.WriteLine($"[{nodeId}] Node stopped gracefully.");
};

Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true; // Prevent immediate termination
    Console.WriteLine($"[{nodeId}] Received CTRL+C. Starting graceful shutdown...");
    node.Stop();
    Console.WriteLine($"[{nodeId}] Node stopped gracefully.");
    Environment.Exit(0);
};

// Register shutdown hook with ASP.NET Core lifetime events
lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine($"[{nodeId}] Application stopping. Initiating node shutdown...");
    node.Stop();
    Console.WriteLine($"[{nodeId}] Node shutdown complete.");
});

await node.StartAsync();

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


// Optionally, setup WebSocket chat room (if applicable)
/// var webSocketRoom = new ChatRoom();
// var webSocketServerService = new WebSocketServerService();
// Task.Run(() => webSocketServerService.StartWebSocketServer(webSocketRoom));

app.Run();
