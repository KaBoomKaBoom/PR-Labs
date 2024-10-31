using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

public class TCPService
{
    private TcpListener server;

    public TCPService(int port)
    {
        server = new TcpListener(IPAddress.Any, port);
    }

    public void Start()
    {
        server.Start();
        Console.WriteLine("Server started at port: " + ((IPEndPoint)server.LocalEndpoint).Port);

        while(true)
        {
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Client connected");
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    public void HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];
            int byteCount;

            while((byteCount = stream.Read(buffer, 0 , buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                Console.WriteLine("Message received: " + message);

                if(message.StartsWith("WRITE"))
                {
                    FileService.IncrementWriteCount();
                    Thread writeThread = new Thread(() => FileService.WriteToFile(message.Substring(6)));
                    writeThread.Start();
                }
                else if(message.StartsWith("READ"))
                {
                    //Wait until all write operations are completed
                    FileService.WaitForWritesToComplete();
                    Thread readThread = new Thread(() => FileService.ReadFromFile());
                    readThread.Start();
                }
            }
        }
        client.Close();
    }
}