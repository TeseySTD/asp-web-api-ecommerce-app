using EcommerceProject.API.Extensions;
using EcommerceProject.Persistence.Extensions;
using EcommerceProject.Application.Extensions;
using EcommerceProject.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGenWithAuthScheme();
//From extensions
services
    .AddApplicationLayerServices()
    .AddInfrastructureLayerServices()
    .AddPersistenceLayerServices(builder.Configuration);

services.AddAuthentication(builder.Configuration);
services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerDarkThemeUi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
