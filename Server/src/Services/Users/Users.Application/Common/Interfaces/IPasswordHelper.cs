
namespace Users.Application.Common.Interfaces;

public interface IPasswordHelper
{
    public string HashPassword(string password);
    public bool VerifyPassword(string hashedPassword, string password);
}