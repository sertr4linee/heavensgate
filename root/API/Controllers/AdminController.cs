using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ITokenCleanupService _tokenCleanupService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ITokenCleanupService tokenCleanupService, ILogger<AdminController> logger)
        {
            _tokenCleanupService = tokenCleanupService;
            _logger = logger;
        }

        [HttpPost("cleanup-tokens")]
        public async Task<IActionResult> CleanupTokens()
        {
            try
            {
                var expiredCount = await _tokenCleanupService.GetExpiredTokensCountAsync();
                await _tokenCleanupService.CleanupExpiredTokensAsync();
                
                return Ok(new { 
                    message = $"Cleaned up {expiredCount} expired tokens",
                    cleanedTokens = expiredCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual token cleanup");
                return StatusCode(500, "Error during token cleanup");
            }
        }
    }
} 