using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lab1.Services
{
    public class RequestSiteService
    {
        private string _siteName;

        public RequestSiteService()
        {
            _siteName = "https://darwin.md/telefoane";
        }

        public async Task GetSiteContent()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Send an HTTP GET request to the specified URL
                    HttpResponseMessage response = await client.GetAsync(_siteName);
                    
                    // Check if the response was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the HTML content of the response
                        string htmlContent = await response.Content.ReadAsStringAsync();
                    
                        // Output the HTML content
                        Console.WriteLine("HTML Content:\n");
                        Console.WriteLine(htmlContent);
                        
                        // Save content to a file
                        string filePath = "siteContent.html"; // Specify your file path here
                        await SaveContentToFile(htmlContent, filePath);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the request
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        private async Task SaveContentToFile(string content, string filePath)
        {
            try
            {
                // Write the content to the specified file
                await File.WriteAllTextAsync(filePath, content);
                Console.WriteLine($"Content saved to {filePath}");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during file writing
                Console.WriteLine($"An error occurred while saving the file: {ex.Message}");
            }
        }
    }
}
