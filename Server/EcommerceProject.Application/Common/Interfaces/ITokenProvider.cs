using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.Entities;

namespace EcommerceProject.Application.Common.Interfaces;

public interface ITokenProvider
{
    public string GenerateJwtToken(User user);
    public RefreshToken GenerateRefreshToken(User user);
}