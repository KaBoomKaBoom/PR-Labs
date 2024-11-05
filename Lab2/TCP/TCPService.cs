using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

public class TCPService
{
    private TcpListener server;

    public TCPService(int port)
    {
        //Create a new TCP server
        server = new TcpListener(IPAddress.Any, port);
    }

    public void Start()
    {
        //Start the server
        server.Start();
        Console.WriteLine("Server started at port: " + ((IPEndPoint)server.LocalEndpoint).Port);

        while(true)
        {
            //Accept incoming client connections
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Client connected");

            //Handle client in a separate thread
            Thread clientThread = new Thread(() => HandleClient(client));
            clientThread.Start();
        }
    }

    public void HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            //storre incoming data
            byte[] buffer = new byte[1024];
            int byteCount;

            while((byteCount = stream.Read(buffer, 0 , buffer.Length)) > 0)
            {
                //Convert the incoming data to a string
                string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                Console.WriteLine("Message received: " + message);

                if(message.StartsWith("WRITE"))
                {
                    FileService.IncrementWriteCount();
                    //Start a new thread to handle the write operation
                    Thread writeThread = new Thread(() => FileService.WriteToFile(message.Substring(6)));
                    writeThread.Start();
                }
                else if(message.StartsWith("READ"))
                {
                    //Wait until all write operations are completed
                    FileService.WaitForWritesToComplete();
                    //Start a new thread to handle the read operation
                    Thread readThread = new Thread(() => FileService.ReadFromFile());
                    readThread.Start();
                }
            }
        }
        //Close the client connection
        client.Close();
    }
}