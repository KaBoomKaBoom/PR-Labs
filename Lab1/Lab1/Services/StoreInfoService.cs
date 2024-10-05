using HtmlAgilityPack;
using Lab1.Models;

namespace Lab1.Services
{
    public class StoreInfoService
    {
        private ExtractProductService _extractProduct;
        private List<Product> _products;
        public StoreInfoService()
        {
            _extractProduct = new ExtractProductService();
            _products = new List<Product>();
        }

        public void StoreInfo(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var productNodes = htmlDoc.DocumentNode.SelectNodes("//figure[contains(@class, 'card-product')]");
            if (productNodes != null)
            {
                foreach (var productNode in productNodes)
                {
                    var product = new Product
                    {
                        Name = _extractProduct.ExtractProductName(productNode),
                        Price = _extractProduct.ExtractProductPrice(productNode),
                        Link = _extractProduct.ExtractProductLink(productNode)
                    };
                    Console.WriteLine($"Product: {product.Name}, Price: {product.Price}, Link: {product.Link}");
                    _products.Add(product);
                }
            }
            else
            {
                Console.WriteLine("No products found.");
            }
        }
    }
}