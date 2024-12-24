using EcommerceProject.Persistence;
using Microsoft.EntityFrameworkCore;
using EcommerceProject.Persistence.Extensions;
using EcommerceProject.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
//From extensions
services
    .AddApplicationLayerServices()
    .AddPersistenceLayerServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.InjectStylesheet("/swagger-ui/_base.css");
        c.InjectStylesheet("/swagger-ui/theme.css");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
