using System.Reflection;
using Carter;
using Shared.Core.Extensions;
using Users.API.Extensions;
using Users.Application;
using Users.Infrastructure.Extensions;
using Users.Persistence;
using Users.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGenWithAuthScheme();

services
    .AddApplicationLayerServices()
    .AddPersistenceLayerServices(builder.Configuration)
    .AddInfrastructureLayerServices();

services.AddCarter();

services.AddAuthentication(builder.Configuration);
services.AddAuthrorizationWithRoleHierarchyPolicies();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerDarkThemeUi();
    app.ApplyMigrations();
}
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.Run();