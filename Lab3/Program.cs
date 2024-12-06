using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Stream.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var ftpDownloader = new FtpDownloader();
        var server = new Server();

        var streamSystemConfig = new StreamSystemConfig
        {
            Endpoints = new List<EndPoint> { new DnsEndPoint("localhost", 5552) },
            UserName = "guest",
            Password = "guest"
        };

        var streamSystem = await StreamSystem.Create(streamSystemConfig);

        // Start the consumer
        var streamConsumer = new ConsumeFromRMQ();
        streamConsumer.Consume(streamSystem);

        // Start a separate thread to download files every 30 seconds
        Thread downloadThread = new Thread(() =>
        {
            var count = 2;
            while (true)
            {
                try
                {
                    Console.WriteLine("Downloading file...");
                    var filePath = ftpDownloader.DownloadFile($"ftp://localhost:2121/productsInEuro-{count}.json", "user", "pass", $"productsInEuro-{count}.json").Result;

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        Console.WriteLine("Uploading file...");
                        server.SendMultipartPostRequest(filePath).Wait();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in download/upload thread: {ex.Message}");
                }
                count++;
                Thread.Sleep(10000); // Wait for 30 seconds
            }
        });

        downloadThread.IsBackground = true; // Ensure thread exits when main thread exits
        downloadThread.Start();

        // Optional: Keep the application running
        await Task.Delay(Timeout.Infinite);
    }
}
