using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace API.Services
{
    public interface ITokenCleanupService
    {
        Task CleanupExpiredTokensAsync();
        Task<int> GetExpiredTokensCountAsync();
    }

    public class TokenCleanupService : ITokenCleanupService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TokenCleanupService> _logger;

        public TokenCleanupService(AppDbContext context, ILogger<TokenCleanupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CleanupExpiredTokensAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var count = await _context.RefreshTokens
                    .Where(t => t.ExpiryDate < now || !t.IsActive)
                    .ExecuteDeleteAsync();

                if (count > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired tokens", count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token cleanup");
                throw;
            }
        }

        public async Task<int> GetExpiredTokensCountAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.RefreshTokens
                .CountAsync(t => t.ExpiryDate < now || !t.IsActive);
        }
    }
} 