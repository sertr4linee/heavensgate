using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using API.Extensions;

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
            if (HttpMethods.IsGet(context.Request.Method))
            {
                await _next(context);
                return;
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                _logger.LogTransaction(context.Request.Path, "Début");
                
                await _next(context);
                
                if (dbContext.ChangeTracker.HasChanges())
                {
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    _logger.LogTransaction(context.Request.Path, "Validée");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
} 