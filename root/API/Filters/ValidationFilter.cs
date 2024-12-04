using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray() ?? Array.Empty<string>()
                    );

                context.Result = new BadRequestObjectResult(new
                {
                    Message = "Validation errors",
                    Errors = errors
                });
                return;
            }

            await next();
        }
    }
} 