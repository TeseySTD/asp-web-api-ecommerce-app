using Basket.API.Application;
using Basket.API.Application.UseCases;
using Basket.API.Data;
using Carter;
using Shared.Core.Auth;
using Shared.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuthScheme();

services.AddCarter();

services.AddSharedAuthentication(Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY_PATH")!);
services.AddAuthrorizationWithRoleHierarchyPolicies();

services
    .AddDataLayerServices(builder.Configuration)
    .AddApplicationLayerServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerDarkThemeUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.Run();