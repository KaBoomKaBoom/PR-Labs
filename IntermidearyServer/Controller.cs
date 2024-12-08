using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]

public class Controller : ControllerBase
{
    [HttpPost("/host/{host}")]
    public IActionResult Host(string host)
    {
        Console.WriteLine($"Host: {host}");
        GlobalState.Host = host;
        return Ok();
    }
}