using Microsoft.AspNetCore.Builder;

namespace Shared.Core.Extensions;

public static class Swagger
{
    public static IApplicationBuilder  UseSwaggerDarkThemeUI(this IApplicationBuilder app)
    {
        app.UseSwaggerUI(options =>
        {
            options.InjectStylesheet("/swagger-dark-theme/_base.css");
            options.InjectStylesheet("/swagger-dark-theme/theme.css");
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
}