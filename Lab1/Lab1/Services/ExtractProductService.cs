using System.Runtime.CompilerServices;
using HtmlAgilityPack;

namespace Lab1.Services
{
    public class ExtractProductService
    {          
        public string ExtractProductName(HtmlNode productNode)
        {   
            var nameNode = productNode.SelectSingleNode(".//div[contains(@class, 'grid-item')]//figcaption//a[contains(@class, 'ga-item')]");
            string name = nameNode != null ? nameNode.InnerText.Trim() : "Name not found";
            return name;
        }
        public string ExtractProductPrice(HtmlNode productNode)
        {   
            var priceNode = productNode.SelectSingleNode(".//div[contains(@class, 'bottom-wrap')]//div[contains(@class, 'price-wrap')]//span[contains(@class, 'price-new')]/b");
            string price = priceNode != null ? priceNode.InnerText.Trim() : "Name not found";
            return price;
        }

        public string ExtractProductLink(HtmlNode productNode)
        {
            // Extract product name
            var nameNode = productNode.SelectSingleNode(".//div[contains(@class, 'grid-item')]//figcaption//a[contains(@class, 'ga-item')]");
            string link = nameNode != null ? nameNode.GetAttributeValue("href", string.Empty) : "Link not found";
            return link;
        }

    }
}