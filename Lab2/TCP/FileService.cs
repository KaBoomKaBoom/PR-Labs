public class FileService
{
    // Mutex to lock file access
    private static Mutex fileMutex = new Mutex();
    // Semaphore to signal that all write operations are finished
    private static SemaphoreSlim writeSemaphore = new SemaphoreSlim(0);
    private static int writeCount = 0; // Keeps track of active write threads
    private const string FilePath = "sharedFile.txt";

    public static void WriteToFile(string data)
    {
        //Aquire the mutex
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
            //Release the mutex
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
        //Aquire the mutex
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
            //Release the mutex
            fileMutex.ReleaseMutex();
        }
    }
    public static void IncrementWriteCount()
    {
        //Increment number of write operations
        Interlocked.Increment(ref writeCount);
    }

    public static void WaitForWritesToComplete()
    {
        //Wait for all write operations to finish
        writeSemaphore.Wait();
    }
}