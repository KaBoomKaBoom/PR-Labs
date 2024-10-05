using HtmlAgilityPack;

namespace Lab1.Services
{
    public class ExtractProductService
    {

        public ExtractProductService()
        {
        }

        public void ExtractProductsInfo(string htmlContent)
        {   
            // Load the HTML document
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var productNodes = htmlDoc.DocumentNode.SelectNodes("//figure[contains(@class, 'card-product')]");
            if (productNodes != null)
            {
                foreach (var productNode in productNodes)
                {
                    // Extract product name
                    var nameNode = productNode.SelectSingleNode(".//div[contains(@class, 'grid-item')]//figcaption//a[contains(@class, 'ga-item')]");
                    string name = nameNode != null ? nameNode.InnerText.Trim() : "Name not found";

                    // Extract product name
                    var priceNode = productNode.SelectSingleNode(".//div[contains(@class, 'bottom-wrap')]//div[contains(@class, 'price-wrap')]//span[contains(@class, 'price-new')]/b");
                    string price = priceNode != null ? priceNode.InnerText.Trim() : "Name not found";


                    Console.WriteLine($"Product: {name}, Price: {price}");
                }
            }
            else
            {
                Console.WriteLine("No products found.");
            }
        }

        public void ExtractLink(string htmlContent)
        {
            // Load the HTML document
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var productNodes = htmlDoc.DocumentNode.SelectNodes("//figure[contains(@class, 'card-product')]");
            if (productNodes != null)
            {
                foreach (var productNode in productNodes)
                {
                    // Extract product name
                    var nameNode = productNode.SelectSingleNode(".//div[contains(@class, 'grid-item')]//figcaption//a[contains(@class, 'ga-item')]");
                    string name = nameNode != null ? nameNode.InnerText.Trim() : "Name not found";

                    // Extract product name
                    var priceNode = productNode.SelectSingleNode(".//div[contains(@class, 'bottom-wrap')]//div[contains(@class, 'price-wrap')]//span[contains(@class, 'price-new')]/b");
                    string price = priceNode != null ? priceNode.InnerText.Trim() : "Name not found";


                    Console.WriteLine($"Product: {name}, Price: {price}");
                }
            }
            else
            {
                Console.WriteLine("No products found.");
            }
        }

    }
}