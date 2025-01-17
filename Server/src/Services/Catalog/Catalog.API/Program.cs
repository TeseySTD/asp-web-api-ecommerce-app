using Carter;
using Catalog.API;
using Catalog.Application;
using Catalog.Persistence;
using Catalog.Persistence.Extensions;
using Shared.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services
    .AddApplicationLayerServices(builder.Configuration)
    .AddPersistenceLayerServices(builder.Configuration);

services.AddCarter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerDarkThemeUI();
    app.ApplyMigrations();
}

app.UseHttpsRedirection();
app.MapCarter();

app.Run();