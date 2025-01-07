using EcommerceProject.Core.Models.Users;

namespace EcommerceProject.Application.Common.Interfaces;

public interface IJwtTokenProvider
{
    public string GenerateToken(User user);
}