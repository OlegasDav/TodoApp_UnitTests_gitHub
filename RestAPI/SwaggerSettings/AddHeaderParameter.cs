using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RestAPI.SwaggerSettings
{
    public class AddHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;

            if (descriptor != null && 
                !descriptor.ControllerName.StartsWith("Auth") && 
                !descriptor.ControllerName.StartsWith("ApiKeys"))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "ApiKey",
                    In = ParameterLocation.Header,
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "String"
                    }
                });
            }
        }
    }
}


// if (operation.Parameters == null)
//     operation.Parameters = new List<OpenApiParameter>();
//
// var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
//
// if (descriptor != null && !descriptor.ControllerName.StartsWith("Auth") && !descriptor.ControllerName.StartsWith("ApiKeys"))
// {
//     operation.Parameters.Add(new OpenApiParameter
//     {
//         Name = "ApiKey",
//         In = ParameterLocation.Header,
//         Required = false,
//         Schema = new OpenApiSchema
//         {
//             Type = "String"
//         }
//     });
// }