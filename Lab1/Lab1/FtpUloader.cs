using System;
using System.IO;
using System.Net;

public class FtpUploader
{
    public void UploadFile(string ftpUrl, string username, string password, string filePath)
    {
        try
        {
            // Disable Expect 100-Continue header
            ServicePointManager.Expect100Continue = false;
            string fileName = Path.GetFileName(filePath);
            string uploadUrl = $"{ftpUrl}/{fileName}";
            Console.WriteLine($"Uploading file to: {uploadUrl}");
            using (WebClient client = new WebClient())
            {
                // Set credentials
                client.Credentials = new NetworkCredential(username, password);

                // Upload file
                client.UploadFile(uploadUrl, filePath);

                Console.WriteLine("Upload complete");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
