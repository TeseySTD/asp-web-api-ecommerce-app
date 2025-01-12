using System.Reflection;
using Carter;
using Shared.Core.Extensions;
using Users.Application;
using Users.Infrastructure.Extensions;
using Users.Persistence;
using Users.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services
    .AddPersistenceLayerServices(builder.Configuration)
    .AddInfrastructureLayerServices()
    .AddApplicationLayerServices();

services.AddCarter();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}

app.UseHttpsRedirection();
app.MapCarter();

app.Run();