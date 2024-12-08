using Lab2.Helpers;
using Lab2.Models;
using Lab2.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.InteropServices;

GlobalState.PeerCount = 3;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.WebHost.UseUrls("http://*:8080", "http://*:5000");
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
foreach (var peer in peers)
{
    Console.WriteLine($"Peer: {peer}");
}
var app = builder.Build();

//RAFT election
Task.Run(async () =>
{
    // Create and start the Raft node
    var node = new RaftNode(nodeId, nodePort, peers);
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

    Console.WriteLine($"Starting node {nodeId} on port {nodePort}");

    // Handle both SIGTERM (Docker) 
    AppDomain.CurrentDomain.ProcessExit += (s, e) =>
    {
        node.Stop();
    };

    await Task.Delay(5000);
    node.StartAsync();
});

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