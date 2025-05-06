using Shared.Core.Validation.Result;
using Users.API.Endpoints;
using Users.Application.Common.Interfaces;
using Users.Core.Models.Entities;

namespace Users.API.Helpers;

public class EmailVerificationLinkFactory(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
    : IEmailVerificationLinkFactory
{
    public Result<string> Create(EmailVerificationToken token)
    {
        string? verificationLink =
            linkGenerator.GetUriByName(
                httpContextAccessor.HttpContext,
                AuthenticationModule.VerificationEmailName,
                new { tokenId = token.Id.Value }
            );

        return verificationLink is not null
            ? Result<string>.Success(verificationLink!)
            : new Error("Verification link not found", "Verification link not found");
    }
}