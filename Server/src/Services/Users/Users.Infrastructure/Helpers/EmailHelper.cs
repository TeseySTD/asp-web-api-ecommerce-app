using FluentEmail.Core;
using Users.Application.Common.Interfaces;

namespace Users.Infrastructure.Helpers;

public class EmailHelper(IFluentEmail fluentEmail) : IEmailHelper
{
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        await fluentEmail
            .To(email)
            .Subject(subject)
            .Body(message, isHtml: true)
            .SendAsync();
    }
}