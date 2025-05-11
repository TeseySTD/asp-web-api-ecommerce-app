using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Core.Auth;
using Shared.Core.Extensions;
using YarpGateway.Swagger;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGenWithAuthScheme();

services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

services.AddAuthrorizationWithRoleHierarchyPolicies();
services.AddSharedAuthentication(Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY_PATH")!);


services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("sliding", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.SegmentsPerWindow = 6;
    });
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            await context.HttpContext.Response.WriteAsync(
                $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds.",
                token);
        }
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerDarkThemeUI(options =>
    {
        options.SwaggerEndpoint("/users-api/swagger/v1/swagger.json", "Users Api");
        options.SwaggerEndpoint("/catalog-api/swagger/v1/swagger.json", "Catalog Api");
        options.SwaggerEndpoint("/ordering-api/swagger/v1/swagger.json", "Ordering Api");
        options.SwaggerEndpoint("/basket-api/swagger/v1/swagger.json", "Basket Api");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGetSwaggerForYarp(builder.Configuration);
app.MapReverseProxy();

app.Run();

