using System.Reflection;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Users.Tests.Integration.Common;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbConnectionString;
    private readonly string _messageBrokerConnectionString;
    private readonly string _messageBrokerUserName;
    private readonly string _messageBrokerPassword;
    private readonly string _smtpHost;
    private readonly string _smtpPort;
    private readonly string _emailSender;
    private readonly string _emailName;

    public IntegrationTestWebApplicationFactory(
        DatabaseFixture databaseFixture,
        MessageBrokerFixture messageBrokerFixture,
        SmtpServerFixture smtpServerFixture)
    {
        _dbConnectionString = databaseFixture.ConnectionString;
        _messageBrokerConnectionString = messageBrokerFixture.ConnectionString;
        _messageBrokerUserName = messageBrokerFixture.UserName;
        _messageBrokerPassword = messageBrokerFixture.Password;
        _smtpHost = smtpServerFixture.Host;
        _smtpPort = smtpServerFixture.Port;
        _emailSender = smtpServerFixture.SenderEmail;
        _emailName = smtpServerFixture.Sender;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSolutionRelativeContentRoot("src/Services/Users/Users.API");

        builder.ConfigureAppConfiguration(cfg => { cfg.AddEnvironmentVariables(); });

        builder.UseSetting("ConnectionStrings:Database", _dbConnectionString);

        builder.UseSetting("MessageBroker:Host", _messageBrokerConnectionString);
        builder.UseSetting("MessageBroker:UserName", _messageBrokerUserName);
        builder.UseSetting("MessageBroker:Password", _messageBrokerPassword);

        builder.UseSetting("Email:Host", _smtpHost);
        builder.UseSetting("Email:Port", _smtpPort);
        builder.UseSetting("Email:SenderEmail", _emailSender);
        builder.UseSetting("Email:Sender", _emailName);

        Environment.SetEnvironmentVariable("JWT_PUBLIC_KEY_PATH",
            "D:/projects/ASP/WebApiEcommerceProject/server/secrets/jwt_public_key.pem");
        Environment.SetEnvironmentVariable("JWT_PRIVATE_KEY_PATH",
            "D:/projects/ASP/WebApiEcommerceProject/Server/secrets/jwt_private_key.pem");
        
    }
        
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        return base.CreateHost(builder);
    }
    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e =>
                   e is ObjectDisposedException od && od.ObjectName == "EventLogInternal"))
        {
            // Ignore logging error 
        }
    }
}