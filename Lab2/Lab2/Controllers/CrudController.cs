using Lab2.Helpers;
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
        
    }
}