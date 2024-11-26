using EcommerceProject.Persistence;
using Microsoft.EntityFrameworkCore;
using EcommerceProject.Persistence.Extensions;
using EcommerceProject.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//From extensions
builder.Services
    .AddApplicationLayerServices()
    .AddPersistenceLayerServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
