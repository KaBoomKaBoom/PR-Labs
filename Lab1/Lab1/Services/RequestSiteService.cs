using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Lab1.Services
{
    public class RequestSiteService
    {
        private string _siteName;

        public RequestSiteService()
        {
            _siteName = "https://darwin.md/telefoane";
        }

        public async Task<string> GetSiteContent()
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
                        
                        // Save content to a file
                        string filePath = "siteContent.html";
                        await SaveContentToFile(htmlContent, filePath);
                        return htmlContent;

                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        return string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during the request
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return string.Empty;
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
