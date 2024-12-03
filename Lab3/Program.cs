using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using RabbitMQ.Stream.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var ftpDownloader = new FtpDownloader();
        await ftpDownloader.DownloadFile("ftp://localhost:21/productsInEuro.json", "user", "pass", "productsInEuro.json");

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

        // Optional: Keep the application running
        await Task.Delay(Timeout.Infinite);
    }
}