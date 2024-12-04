using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Extensions
{
    public static class MigrationExtensions
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<AppDbContext>>();
                var context = services.GetRequiredService<AppDbContext>();

                try
                {
                    var pendingMigrations = context.Database.GetPendingMigrations().ToList();
                    
                    if (pendingMigrations.Any())
                    {
                        var migrationMessages = new List<string>
                        {
                            "Checking database...",
                            $"Pending migrations: {pendingMigrations.Count}"
                        };
                        migrationMessages.AddRange(pendingMigrations.Select(m => $"- {m}"));

                        logger.LogBox(LogLevel.Information,
                            "DATABASE MIGRATION",
                            migrationMessages.ToArray()
                        );

                        context.Database.Migrate();
                        logger.LogBox(LogLevel.Information,
                            "DATABASE MIGRATION",
                            "Migrations applied successfully"
                        );
                    }
                    else
                    {
                        logger.LogBox(LogLevel.Information,
                            "DATABASE MIGRATION",
                            "Database is up to date"
                        );
                    }

                    if (!context.Roles.Any())
                    {
                        logger.LogBox(LogLevel.Information,
                            "DATABASE MIGRATION",
                            "Creating default roles..."
                        );
                        
                        var roles = new[]
                        {
                            new Microsoft.AspNetCore.Identity.IdentityRole 
                            { 
                                Name = "Admin", 
                                NormalizedName = "ADMIN",
                                ConcurrencyStamp = Guid.NewGuid().ToString()
                            },
                            new Microsoft.AspNetCore.Identity.IdentityRole 
                            { 
                                Name = "User", 
                                NormalizedName = "USER",
                                ConcurrencyStamp = Guid.NewGuid().ToString()
                            }
                        };

                        context.Roles.AddRange(roles);
                        context.SaveChanges();
                        
                        logger.LogBox(LogLevel.Information,
                            "DATABASE MIGRATION",
                            "Roles created: Admin, User"
                        );
                    }

                    var userCount = context.Users.Count();
                    var roleCount = context.Roles.Count();
                    
                    logger.LogBox(LogLevel.Information,
                        "DATABASE MIGRATION",
                        "Database statistics:",
                        $"- Users: {userCount}",
                        $"- Roles: {roleCount}"
                    );

                    logger.LogBox(LogLevel.Information,
                        "DATABASE MIGRATION",
                        "Application ready to use"
                    );
                }
                catch (Exception ex)
                {
                    logger.LogBox(LogLevel.Error,
                        "CRITICAL ERROR",
                        ex.Message
                    );
                    throw;
                }

                return host;
            }
        }
    }
} 