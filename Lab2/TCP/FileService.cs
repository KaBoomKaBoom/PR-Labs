public class FileService
{
    private static Mutex fileMutex = new Mutex();
    private static SemaphoreSlim writeSemaphore = new SemaphoreSlim(0);
    private static int writeCount = 0; // Keeps track of active write threads
    private const string FilePath = "sharedFile.txt";

    public static void WriteToFile(string data)
    {
        fileMutex.WaitOne();

        try
        {
            Random rnd = new Random();
            Thread.Sleep(rnd.Next(1000, 5000)); // Simulate a delay for write operation

            File.AppendAllText(FilePath, data + Environment.NewLine);
            Console.WriteLine($"Data written to file: {data}");
        }
        finally
        {
            fileMutex.ReleaseMutex();

            //Decrement number of write operations and check
            if (Interlocked.Decrement(ref writeCount) == 0)
            {
                //Signal that all writes operations are finished
                writeSemaphore.Release();
            }
        }
    }

    public static void ReadFromFile()
    {
        fileMutex.WaitOne();

        try
        {
            Random rnd = new Random();

            // Simulate a delay for read operation
            Thread.Sleep(rnd.Next(1000, 5000));

            string fileContent = File.ReadAllText(FilePath);
            Console.WriteLine($"File content: {fileContent}");
        }
        finally
        {
            fileMutex.ReleaseMutex();
        }
    }
    public static void IncrementWriteCount()
    {
        Interlocked.Increment(ref writeCount);
    }

    public static void WaitForWritesToComplete()
    {
        writeSemaphore.Wait();
    }
}