using Users.Core.Models;
using Users.Core.Models.Entities;

namespace Users.Application.Common.Interfaces;

public interface ITokenProvider
{
    public string GenerateJwtToken(User user);
    public RefreshToken GenerateRefreshToken(User user);
}