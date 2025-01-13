using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Users.API.Extensions;

public static class Swagger
{
    public static IServiceCollection AddSwaggerGenWithAuthScheme(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
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

}