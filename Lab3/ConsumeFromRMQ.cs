using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;

public class ConsumeFromRMQ
{
    public async void Consume(StreamSystem streamSystem)
    {
        var streamName = "products";
        
        // Ensure the stream exists
        if (!await streamSystem.StreamExists(streamName))
        {
            Console.WriteLine($"Stream {streamName} does not exist.");
            return;
        }

        var consumerConfig = new ConsumerConfig(streamSystem, streamName)
        {
            OffsetSpec = new OffsetTypeFirst(),
            MessageHandler = async (stream, _, _, message) =>
            {
                Console.WriteLine($"Stream: {stream} - " +
                                  $"Received message: {Encoding.UTF8.GetString(message.Data.Contents)}");
                await Task.CompletedTask;
            }
        };
        var consumer = await Consumer.Create(consumerConfig);
        
        // Keep the consumer running
        Console.WriteLine("Consumer started. Press any key to exit.");
        Console.ReadKey();

        // Close the consumer when done
        await consumer.Close();
    }
}
