using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Linq;

namespace WebAPI.Swagger
{
    /// <summary>
    /// Operation filter to add authentication information to the operation summary
    /// </summary>
    public class OperationSummaryFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Get authentication requirements
            var hasAuth = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any() ||
                          context.MethodInfo.GetCustomAttributes(true).OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Any();
            
            if (!hasAuth)
                return;
                
            // Get any roles required
            var authorizeAttributes = context.MethodInfo.GetCustomAttributes(true)
                .Union(context.MethodInfo.DeclaringType.GetCustomAttributes(true))
                .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
            
            var requiredRoles = authorizeAttributes
                .Where(attr => !string.IsNullOrEmpty(attr.Roles))
                .SelectMany(attr => attr.Roles.Split(','))
                .Distinct()
                .ToList();
            
            // Simply append [ADMIN] or [AUTH] to the summary
            if (requiredRoles.Any(r => r.Contains("Admin")))
            {
                operation.Summary = $"{operation.Summary} [ADMIN]";
            }
            else if (hasAuth)
            {
                operation.Summary = $"{operation.Summary} [AUTH]";
            }
        }
    }
} 