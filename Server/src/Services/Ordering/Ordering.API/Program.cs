using Carter;
using Ordering.Application;
using Ordering.Persistence;
using Ordering.Persistence.Extensions;
using Shared.Core.Auth;
using Shared.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGenWithAuthScheme();

services
    .AddApplicationLayerServices(builder.Configuration)
    .AddPersistenceLayerServices(builder.Configuration);

services.AddCarter();

services.AddSharedAuthentication(Environment.GetEnvironmentVariable("JWT_PUBLIC_KEY_PATH")!);
services.AddAuthrorizationWithRoleHierarchyPolicies();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerDarkThemeUI();
    if (app.Environment.EnvironmentName != "Testing")
        app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.Run();

public partial class Program
{
}