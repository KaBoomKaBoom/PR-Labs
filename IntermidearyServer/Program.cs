using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Stream.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.WebHost.UseUrls("http://*:6000", "http://*:8000");

builder.Services.AddCors((options) =>
    {
        options.AddPolicy("DevCors", (corsBuilder) =>
            {
                corsBuilder.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000", "http://localhost:5000", "http://localhost:8080")
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
var maxRetries = 5;
var streamConsumer = new ConsumeFromRMQ();
Thread rmqThread = new Thread(async () =>
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Task.Delay(3000).Wait(); // Wait for RabbitMQ to start
            var streamSystemConfig = new StreamSystemConfig
            {
                Endpoints = new List<EndPoint> { new DnsEndPoint("host.docker.internal", 5552) },
                UserName = "guest",
                Password = "guest"
            };

            var streamSystem = await StreamSystem.Create(streamSystemConfig);

            streamConsumer.Consume(streamSystem);

            Console.WriteLine("Successfully connected to RabbitMQ");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RabbitMQ Connection Attempt {attempt} Failed: {ex.Message}");

            if (attempt < maxRetries)
            {
                // Exponential backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            }
            else
            {
                Console.WriteLine("Failed to connect to RabbitMQ after all retry attempts.");
                throw;
            }
        }
    }
});
rmqThread.Start();

Thread downloadThread = new Thread(() =>
        {
            var ftpDownloader = new FtpDownloader();
            var server = new Server();
            var count = 2;
            while (true)
            {
                Thread.Sleep(30000); // Wait for 30 seconds
                try
                {
                    Console.WriteLine("Downloading file...");
                    var filePath = ftpDownloader.DownloadFile($"ftp://host.docker.internal:2121/productsInEuro-{count}.json", "user", "pass", $"productsInEuro-{count}.json").Result;

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        count++;
                        Console.WriteLine("Uploading file...");
                        server.SendMultipartPostRequest(filePath).Wait();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in download/upload thread: {ex.Message}");
                }

            }
        });

downloadThread.Start();
downloadThread.IsBackground = true; // Ensure thread exits when main thread exits

Thread sendEmailThread = new Thread(() =>
{
    var emailSender = new MailSender();
    while (true)
    {
        try
        {
            Console.WriteLine("Sending email...");
            emailSender.SendMail();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in email thread: {ex.Message}");
        }
        Thread.Sleep(60000); // Wait for 60 seconds
    }
});

sendEmailThread.Start();

app.Run();