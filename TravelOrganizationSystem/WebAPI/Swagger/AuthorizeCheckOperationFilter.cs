using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace WebAPI.Swagger
{
    /// <summary>
    /// Operation filter to add authorization information to Swagger documentation
    /// </summary>
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="context">The current operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Get endpoint metadata for controller and action
            var hasAuthorize = 
                context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ||
                context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (!hasAuthorize) return;

            // Get any roles required
            var authorizeAttributes = context.MethodInfo.GetCustomAttributes(true)
                .Union(context.MethodInfo.DeclaringType.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>();
            
            var requiredRoles = authorizeAttributes
                .Where(attr => !string.IsNullOrEmpty(attr.Roles))
                .SelectMany(attr => attr.Roles.Split(','))
                .Distinct()
                .ToList();

            // Add JWT authentication requirement to operation
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });

            // Add authentication & roles info to description
            var authDescription = "**REQUIRES AUTHENTICATION**";
            if (requiredRoles.Any())
            {
                authDescription += $"\n\nRequired role(s): {string.Join(", ", requiredRoles)}";
            }

            operation.Description = string.IsNullOrEmpty(operation.Description) 
                ? authDescription 
                : $"{operation.Description}\n\n{authDescription}";
        }
    }
} 