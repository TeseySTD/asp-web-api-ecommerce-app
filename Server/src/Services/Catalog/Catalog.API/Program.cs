using Carter;
using Catalog.API;
using Catalog.Application;
using Catalog.Persistence;
using Catalog.Persistence.Extensions;
using Shared.Core.Auth;
using Shared.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGenWithAuthScheme();

services
    .AddApiLayerServices() //Must be before application layer.
    .AddApplicationLayerServices(builder.Configuration)
    .AddPersistenceLayerServices(builder.Configuration);

services.AddCarter();

services.AddSharedAuthentication(Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY_PATH")!);
services.AddAuthrorizationWithRoleHierarchyPolicies();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerDarkThemeUI();
    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.Run();