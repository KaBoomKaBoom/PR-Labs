using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TestClient
{
    private static Random random = new Random();
    private const int ServerPort = 5000;
    private const string ServerAddress = "127.0.0.1";

    static void Main()
    {
        for (int i = 0; i < 5; i++) // Create 5 concurrent clients
        {
            Thread clientThread = new Thread(StartClient);
            clientThread.Start();
        }
    }

    static void StartClient()
    {
        using (TcpClient client = new TcpClient())
        {
            client.Connect(ServerAddress, ServerPort);
            NetworkStream stream = client.GetStream();

            for (int i = 0; i < 10; i++) // Send 10 messages per client
            {
                // Randomly choose between "WRITE" and "READ" command
                bool isWrite = random.Next(2) == 0;
                string message;

                if (isWrite)
                {
                    // Generate a random message to write
                    message = $"WRITE Message from client {Thread.CurrentThread.ManagedThreadId}: {Guid.NewGuid()}";
                }
                else
                {
                    message = "READ";
                }

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Console.WriteLine($"Client {Thread.CurrentThread.ManagedThreadId} sent: {message}");

                // Random delay between 1 and 5 seconds
                Thread.Sleep(random.Next(5000, 10000));
            }
        }
    }
}
