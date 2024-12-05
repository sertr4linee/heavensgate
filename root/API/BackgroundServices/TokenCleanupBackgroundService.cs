using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using API.Services;

namespace API.BackgroundServices
{
    public class TokenCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<TokenCleanupBackgroundService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);

        public TokenCleanupBackgroundService(
            IServiceProvider services,
            ILogger<TokenCleanupBackgroundService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await DoCleanupAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initial cleanup failed");
            }

            using var timer = new PeriodicTimer(_cleanupInterval);
            
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await DoCleanupAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during token cleanup");
                }
            }
        }

        private async Task DoCleanupAsync()
        {
            using var scope = _services.CreateScope();
            var cleanupService = scope.ServiceProvider.GetRequiredService<ITokenCleanupService>();

            var expiredCount = await cleanupService.GetExpiredTokensCountAsync();
            if (expiredCount > 0)
            {
                _logger.LogInformation("Starting cleanup of {Count} expired tokens", expiredCount);
                await cleanupService.CleanupExpiredTokensAsync();
            }
        }
    }
} 