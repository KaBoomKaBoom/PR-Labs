using System.Numerics;
using System.Runtime.CompilerServices;
using HtmlAgilityPack;

namespace Lab1.Services
{
    public class ExtractProductService
    {          
        private ValidationService _validationService;

        public ExtractProductService()
        {
            _validationService = new ValidationService();
        }
        public string ExtractProductName(HtmlNode productNode)
        {   
            var nameNode = productNode.SelectSingleNode(".//div[contains(@class, 'grid-item')]//figcaption//a[contains(@class, 'ga-item')]");
            string name = nameNode != null ? nameNode.InnerText.Trim() : "Name not found";
            return name;
        }
        public int ExtractProductPrice(HtmlNode productNode)
        {   
            var priceNode = productNode.SelectSingleNode(".//div[contains(@class, 'bottom-wrap')]//div[contains(@class, 'price-wrap')]//span[contains(@class, 'price-new')]/b");
            string price = priceNode != null ? priceNode.InnerText.Trim() : "Price not found";
            return _validationService.ValidatePrice(price.Trim());
        }

        public string ExtractProductLink(HtmlNode productNode)
        {
            // Extract product name
            var nameNode = productNode.SelectSingleNode(".//div[contains(@class, 'grid-item')]//figcaption//a[contains(@class, 'ga-item')]");
            string link = nameNode != null ? nameNode.GetAttributeValue("href", string.Empty) : "Link not found";
            return link;
        }
        public string ExtractProductResolution(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            var productNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'main-description')]//ul[contains(@class, 'features')]");
            if (productNode != null)
            {
                var features = productNode.SelectNodes(".//li");
                if (features != null)
                {
                    foreach (var feature in features)
                    {
                        if (feature.InnerText.Trim().Contains("Rezolu"))
                        {
                            var resolutionDescription = feature.InnerText.Trim();
                            return _validationService.ValidateResolution(resolutionDescription.Split(":")[1]);
                        }
                    }
                }
            }
            return "Resolution not found";
            // var resolutionNode = productNode.SelectSingleNode(".//div[contains(@class, 'main-description')]//ul[contains(@class, 'features')]//li[contains(@class, 'resolution')]");
            // string resolution = resolutionNode != null ? resolutionNode.InnerText.Trim() : "Resolution not found";
        }

    }
}