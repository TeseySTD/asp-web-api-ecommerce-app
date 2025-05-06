using Carter;
using Shared.Core.Auth;
using Shared.Core.Extensions;
using Users.API;
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
    .AddApplicationLayerServices(builder.Configuration)
    .AddPersistenceLayerServices(builder.Configuration)
    .AddInfrastructureLayerServices(builder.Configuration)
    .AddApiLayerServices(builder.Configuration);

services.AddCarter();

services.AddAuthentication(builder.Configuration);
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