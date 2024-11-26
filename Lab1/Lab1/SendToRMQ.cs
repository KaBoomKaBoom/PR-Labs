using System.Text;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;

public class SendToRMQ
{
    public async void Send(StreamSystem streamSystem, string json)
    {
        var streamName = "products";

        // Ensure the stream exists
        if (!await streamSystem.StreamExists(streamName))
        {
            await streamSystem.CreateStream(new StreamSpec(streamName));
        }

        var producer = await Producer.Create(new ProducerConfig(streamSystem, streamName));
        var message = new Message(Encoding.UTF8.GetBytes(json));

        await producer.Send(message);
        // Console.WriteLine($"Sent message: {json}");

        await producer.Close();
    }
}
