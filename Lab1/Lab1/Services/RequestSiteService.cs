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
                    
                        // // Output the HTML content
                        // Console.WriteLine("HTML Content:\n");
                        // Console.WriteLine(htmlContent);
                        
                        // Save content to a file
                        string filePath = "siteContent.html"; // Specify your file path here
                        await SaveContentToFile(htmlContent, filePath);

                        // Extract products info
                        ExtractProductsInfo(htmlContent);
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

        private void ExtractProductsInfo(string htmlContent)
        {
                    // Load the HTML document
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Select all product nodes based on the provided HTML structure
            var productNameNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'grid-item')]");

            if (productNameNodes != null)
            {
                foreach (var productNameNode in productNameNodes)
                {
                    // Extract product name
                    var nameNode = productNameNode.SelectSingleNode(".//figcaption//a[contains(@class, 'ga-item')]");
                    string name = nameNode != null ? nameNode.InnerText.Trim() : "Name not found";

                    Console.WriteLine($"Product: {name}, ");
                }
            }
            else
            {
                Console.WriteLine("No products found.");
            }
            var productPriceNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'bottom-wrap')]");

            if (productPriceNodes != null)
            {
                foreach (var productPriceNode in productPriceNodes)
                {
                    // Extract product price
                    var priceNode = productPriceNode.SelectSingleNode(".//div[contains(@class, 'price-wrap')]//span[contains(@class, 'price-new')]/b");
                    string price = priceNode != null ? priceNode.InnerHtml.Trim() : "Price not found";

                    Console.WriteLine($"Price: {price}");
                }
            }
            else
            {
                Console.WriteLine("No products found.");
            }
        }
    }
}
