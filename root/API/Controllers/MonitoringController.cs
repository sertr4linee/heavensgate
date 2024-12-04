using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringController : ControllerBase
    {
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { Status = "Healthy" });
        }

        [HttpGet("version")]
        public IActionResult Version()
        {
            return Ok(new { Version = GetType().Assembly.GetName().Version?.ToString() });
        }
    }
} 