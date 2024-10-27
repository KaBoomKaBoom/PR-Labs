using Lab2.DTOs;
using Lab2.Helpers;
using Lab2.Mappers;
using Lab2.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lab2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrudController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly DbCompletition _dbCompletition;

        public CrudController(DataContext context, DbCompletition dbCompletition)
        {
            _context = context;
            _dbCompletition = dbCompletition;
        }

        [HttpGet("/products")]
        public IActionResult GetProducts()
        {
            _dbCompletition.CompleteDb();
            return Ok(_context.Product);
        }

        [HttpGet("/products/{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _context.Product.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost("/products")]
        public IActionResult AddProduct(List<ProductDTO> products)
        {
            var productEntities = products.Select(ProductMapper.MapToProduct).ToList();

            _context.Product.AddRange(productEntities);
            _context.SaveChanges();

            return Created("", productEntities);
        }

        [HttpPut("/products/{id}")]
        public IActionResult UpdateProduct(int id, ProductDTO product)
        {
            var productEntity = _context.Product.Find(id);
            if (productEntity == null)
            {
                return NotFound();
            }

            productEntity.Name = product.Name;
            productEntity.Price = product.Price;
            productEntity.Link = product.Link;
            productEntity.Resolution = product.Resolution;

            _context.SaveChanges();

            return Ok(productEntity);
        }

        [HttpDelete("/products/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Product.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Product.Remove(product);
            _context.SaveChanges();

            return NoContent();
        }
    }

}
