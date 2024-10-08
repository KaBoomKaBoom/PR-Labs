using Lab1.Models;

namespace Lab1.Mappers
{
    public class PriceMapper
    {
        public List<Product> LeiToEuro(List<Product> products)
        {
            // Define the conversion rate
            decimal leiToEuroRate = 0.051m; // Example rate (adjust to actual rate)

            // Map prices from Lei to Euro and round to 2 decimal places
            var productsInEuro = products.Select(product =>
            {
                product.Price = Math.Round(product.Price * leiToEuroRate, 2); // Convert and round price to 2 decimals
                return product;
            }).ToList();
            return productsInEuro;
        }

        public List<Product> FilterProductsByPrice(List<Product> products, decimal minPrice, decimal maxPrice)
        {
            // Filter products by price range
            var filteredProducts = products.Where(product => product.Price >= minPrice && product.Price <= maxPrice).ToList();
            return filteredProducts;
        }
    }
}