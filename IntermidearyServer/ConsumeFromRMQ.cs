using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;

public class ConsumeFromRMQ
{
    private Server _server = new Server();

    public async void Consume(StreamSystem streamSystem)
    {
        var streamName = "products";

        // Ensure the stream exists
        try
        {
            await streamSystem.CreateStream(new StreamSpec(streamName));
            Console.WriteLine($"Stream '{streamName}' created successfully.");
        }
        catch (Exception e) when (e.Message.Contains("stream already exists"))
        {
            Console.WriteLine($"Stream '{streamName}' already exists.");
        }

        var consumerConfig = new ConsumerConfig(streamSystem, streamName)
        {
            OffsetSpec = new OffsetTypeFirst(),
            MessageHandler = async (stream, _, _, message) =>
            {
                var receivedData = Encoding.UTF8.GetString(message.Data.Contents);
                Console.WriteLine($"Stream: {stream} - " +
                                  $"Received message: {receivedData}");
                try
                {
                    await _server.SendPostRequest(receivedData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending post request: {ex.Message}");
                }
                // await Task.CompletedTask;
            }
        };
        var consumer = await Consumer.Create(consumerConfig);

        // Keep the consumer running
        Console.WriteLine("Consumer started.");

        // Close the consumer when done

    }
}
