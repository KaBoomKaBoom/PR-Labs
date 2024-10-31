using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Program
{
    private static Random random = new Random();
    private const int ServerPort = 5000;
    private const string ServerAddress = "127.0.0.1";

    static void Main(string[] args)
    {
        // Check if ClientName argument is provided
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Program <ClientName>");
            return;
        }

        string clientName = args[0]; // Client name provided as argument

        using (TcpClient client = new TcpClient())
        {
            client.Connect(ServerAddress, ServerPort);
            NetworkStream stream = client.GetStream();

            for (int i = 0; i < 10; i++) // Send 10 messages per client instance
            {
                // Randomly choose between "WRITE" and "READ" command
                bool isWrite = random.Next(2) == 0;
                string message;

                if (isWrite)
                {
                    // Generate a random message to write
                    message = $"WRITE Message from {clientName}: {Guid.NewGuid()}";
                }
                else
                {
                    message = $"READ request by {clientName}";
                }

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Console.WriteLine($"{clientName} sent: {message}");

                // Random delay between 1 and 5 seconds
                Thread.Sleep(random.Next(6000, 11000));
            }
        }
    }
}
