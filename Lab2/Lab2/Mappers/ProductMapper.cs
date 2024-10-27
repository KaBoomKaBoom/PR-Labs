using Lab2.DTOs;
using Lab2.Models;

namespace Lab2.Mappers
{
     public static class ProductMapper
    {
        public static Product MapToProduct(ProductDTO dto)
        {
            return new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Link = dto.Link,
                Resolution = dto.Resolution
            };
        }
    }
}