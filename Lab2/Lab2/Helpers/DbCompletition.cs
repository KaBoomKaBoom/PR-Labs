using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Lab2.Models; // Update this namespace to match where your Product entity is located

namespace Lab2.Helpers
{
    public class DbCompletition
    {
        private readonly DataContext _dataContext;

        public DbCompletition(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void CompleteDb()
        {
            _dataContext.Database.EnsureCreated();

            if (!_dataContext.Product.Any()) // Assuming Products DbSet exists in DataContext
            {
                var products = LoadProductsFromJson("Products.json");
                _dataContext.Product.AddRange(products);
                _dataContext.SaveChanges();
            }
        }

        private List<Product> LoadProductsFromJson(string filePath)
        {
            var json = File.ReadAllText(filePath);
#pragma warning disable CS8603 // Possible null reference return.
            return JsonSerializer.Deserialize<List<Product>>(json);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
