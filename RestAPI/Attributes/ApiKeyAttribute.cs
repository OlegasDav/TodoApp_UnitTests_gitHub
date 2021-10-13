using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Persistence;
using Persistence.Repositories;

namespace RestAPI.Attributes
{
    [ApiKey]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var apiKey = context.HttpContext.Request.Headers["ApiKey"].SingleOrDefault();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                context.Result = new BadRequestObjectResult("ApiKey header is missing");
                return;
            }

            var apiKeyRepository = context.HttpContext.RequestServices.GetService<IApiKeysRepository>();

            var apiKeyModel = await apiKeyRepository!.GetByApiKeyAsync(apiKey);

            if (apiKeyModel is null)
            {
                context.Result = new NotFoundObjectResult("ApiKey was not found");
                return;
            }

            if (!apiKeyModel.IsActive)
            {
                context.Result = new UnauthorizedObjectResult("ApiKey is not active");
                return;
            }

            if (apiKeyModel.ExpirationDate <= DateTime.Now)
            {
                context.Result = new UnauthorizedObjectResult("ApiKey is expired");
                return;
            }

            context.HttpContext.Items.Add("userId", apiKeyModel.UserId);
            
            await next();
        }
    }
}