using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Middleware
{
    public class DbTransactionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DbTransactionMiddleware> _logger;

        public DbTransactionMiddleware(RequestDelegate next, ILogger<DbTransactionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // Ignore les requêtes GET car elles ne modifient pas les données
            if (HttpMethods.IsGet(context.Request.Method))
            {
                await _next(context);
                return;
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Début de la transaction pour {Path}", context.Request.Path);
                
                await _next(context);
                
                // Si tout s'est bien passé et qu'il y a des changements, on commit
                if (dbContext.ChangeTracker.HasChanges())
                {
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    _logger.LogInformation("Transaction validée pour {Path}", context.Request.Path);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la transaction pour {Path}", context.Request.Path);
                await transaction.RollbackAsync();
                throw; // L'exception sera gérée par ExceptionMiddleware
            }
        }
    }
} 