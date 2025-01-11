using Microsoft.AspNetCore.Builder;

namespace Shared.Core.Extensions;

public static class Swagger
{
    public static IApplicationBuilder UseSwaggerDarkThemeUi(this IApplicationBuilder app)
    {
        app.UseSwaggerUI(options =>
        {
            options.InjectStylesheet("/swagger-ui/_base.css");
            options.InjectStylesheet("/swagger-ui/theme.css");
        });
        
        // For swagger dark theme style files.
        app.UseStaticFiles();
        
        return app;
    }
}