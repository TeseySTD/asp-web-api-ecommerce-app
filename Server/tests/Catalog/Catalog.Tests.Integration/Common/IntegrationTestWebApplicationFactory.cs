using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Catalog.Tests.Integration.Common;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbConnectionString;
    private readonly string _cacheConnectionString;
    private readonly string _messageBrokerConnectionString;
    private readonly string _messageBrokerUserName;
    private readonly string _messageBrokerPassword;

    public IntegrationTestWebApplicationFactory(
        DatabaseFixture databaseFixture,
        MessageBrokerFixture messageBrokerFixture,
        CacheFixture cacheFixture)
    {
        _dbConnectionString = databaseFixture.ConnectionString;
        _cacheConnectionString = cacheFixture.ConnectionString;
        _messageBrokerConnectionString = messageBrokerFixture.ConnectionString;
        _messageBrokerUserName = messageBrokerFixture.UserName;
        _messageBrokerPassword = messageBrokerFixture.Password;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSolutionRelativeContentRoot("src/Services/Users/Users.API");

        builder.ConfigureAppConfiguration(cfg => { cfg.AddEnvironmentVariables(); });

        builder.UseSetting("ConnectionStrings:Database", _dbConnectionString);
        builder.UseSetting("ConnectionStrings:Redis", _cacheConnectionString);

        builder.UseSetting("MessageBroker:Host", _messageBrokerConnectionString);
        builder.UseSetting("MessageBroker:UserName", _messageBrokerUserName);
        builder.UseSetting("MessageBroker:Password", _messageBrokerPassword);

        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY_PATH",
            "D:/projects/ASP/WebApiEcommerceProject/server/secrets/jwt_public_key.pem");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IConfigureOptions<JwtBearerOptions>>();

            // Configure JwtBearer options without re-adding the scheme
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TestJwtTokens.SecurityKey
                };
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                var busControls = Services.GetServices<IBusControl>().ToArray();
                foreach (var bus in busControls)
                {
                    bus.Stop(); 
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }
        base.Dispose(disposing);
    }

}