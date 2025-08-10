using Testcontainers.Papercut;

namespace Users.Tests.Integration.Common;

public class SmtpServerFixture : IAsyncLifetime
{
    private readonly PapercutContainer _smtpServerContainer = new PapercutBuilder()
        .WithName("users.test.smtp")
        .WithImage("changemakerstudiosus/papercut-smtp:latest")
        .Build();

    public string Port => _smtpServerContainer.SmtpPort.ToString();
    public string Host => _smtpServerContainer.Hostname;
    public string Sender => "users.test.api";
    public string SenderEmail => "users.test.smtp@noreply.com";
    
    public async Task InitializeAsync()
    {
        await _smtpServerContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
       await _smtpServerContainer.StopAsync();
    }
}