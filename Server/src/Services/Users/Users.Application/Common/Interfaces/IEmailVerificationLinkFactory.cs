using Shared.Core.Validation.Result;
using Users.Core.Models.Entities;

namespace Users.Application.Common.Interfaces;

public interface IEmailVerificationLinkFactory
{ 
    Result<string> Create(EmailVerificationToken token);
}