using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Shared.Core.Extensions;

public static class Swagger
{
    public static IApplicationBuilder UseSwaggerDarkThemeUI(
        this IApplicationBuilder app,
        Action<SwaggerUIOptions>? setupAction = null)
    {
        app.UseSwaggerUI(options =>
        {
            // Apply dark theme styles
            options.InjectStylesheet("/swagger-dark-theme/_base.css");
            options.InjectStylesheet("/swagger-dark-theme/theme.css");

            // Apply custom options if provided
            setupAction?.Invoke(options);
        });
        
        // Add middleware to serve embedded resources
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/swagger-dark-theme"))
            {
                var assembly = typeof(Swagger).Assembly;
                var fileName = context.Request.Path.Value!.Split('/').Last();
                var resourceName = $"Shared.Core.SwaggerDarkTheme.{fileName}";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    context.Response.ContentType = "text/css";
                    await stream.CopyToAsync(context.Response.Body);
                    return;
                }
            }

            await next();
        });

        return app;
    }
    
    public static IServiceCollection AddSwaggerGenWithAuthScheme(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<SwaggerFileOperationFilter>();
            options.CustomSchemaIds(id => id.FullName!.Replace("+", "-"));

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter JWT Bearer token",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
            };

            var sequrityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    []
                }
            };
    
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
            options.AddSecurityRequirement(sequrityRequirement);
        });

        return services;
    }
    
    
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formFileParameters = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IEnumerable<IFormFile>) || p.ParameterType == typeof(IFormFile))
                .ToList();

            foreach (var parameter in formFileParameters)
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    { parameter.Name, new OpenApiSchema { Type = "string", Format = "binary" } }
                                }
                            }
                        }
                    }
                };
            }
        }
    }

}