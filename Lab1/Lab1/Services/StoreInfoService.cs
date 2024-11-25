using System.Text.Json;
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

        public List<Product>? StoreInfo(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            var productNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'grid-item uk-box-shadow-hover-medium')]");
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
                    // Console.WriteLine($"Product: {product.Name}, Price: {product.Price}, Link: {product.Link}");
                    _products.Add(product);
                }
                return _products;
            }
            else
            {
                Console.WriteLine("No products found.");
                return null;
            }
        }

        public Product StoreAdditionalInfo(string htmlContent, Product product)
        {
            product.Resolution = _extractProduct.ExtractProductResolution(htmlContent);

            return product;
        }

        public string StoreAsJson(List<Product> products)
        {
            var json = JsonSerializer.Serialize(products);
            return json;
        }

        public List<FilteredProduct> StoreProductsWithTotalPrice(List<Product> products, decimal totalPrice)
        {
            var filteredProducts = new List<FilteredProduct>();
            foreach (var product in products)
            {
                var filteredProduct = new FilteredProduct
                {
                    Name = product.Name,
                    Price = product.Price,
                    Link = product.Link,
                    Resolution = product.Resolution,
                    TotalPriceFilteredProducts = totalPrice,
                    Date = DateTime.Now
                };
                filteredProducts.Add(filteredProduct);
            }
            return filteredProducts;
        }

    }
}