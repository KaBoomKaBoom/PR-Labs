using System.Net;

public class FtpDownloader
{
    public async Task<string> DownloadFile(string ftpUrl, string username, string password, string filePath)
    {
        try
        {
            ServicePointManager.Expect100Continue = false;

            // Step 1: Download the JSON file from FTP
            string fileName = Path.GetFileName(filePath);
            string localFilePath = Path.Combine(Path.GetTempPath(), fileName);

            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(username, password);
                await client.DownloadFileTaskAsync(new Uri(ftpUrl), localFilePath);
            }

            Console.WriteLine($"File downloaded to: {localFilePath}");

            // Step 2: Read the downloaded file as a string
            string fileContent = File.ReadAllText(localFilePath);
            Console.WriteLine($"File content: {fileContent}");

            // Step 3: Delete the file from the FTP server
            DeleteFileFromFtp(ftpUrl, username, password);
            Console.WriteLine($"File deleted from FTP: {ftpUrl}");

            return localFilePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return string.Empty;
        }
    }

    private void DeleteFileFromFtp(string ftpUrl, string username, string password)
    {
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(username, password);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                Console.WriteLine($"Delete status: {response.StatusDescription}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while deleting the file: {ex.Message}");
        }
    }
}