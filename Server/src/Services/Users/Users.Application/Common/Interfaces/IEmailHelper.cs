namespace Users.Application.Common.Interfaces;

public interface IEmailHelper
{
    public Task SendEmailAsync(string email, string subject, string message);
}