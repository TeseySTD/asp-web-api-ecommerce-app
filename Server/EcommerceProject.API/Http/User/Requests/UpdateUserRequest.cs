using EcommerceProject.Application.Dto.User;

namespace EcommerceProject.API.Http.User.Requests;

public record UpdateUserRequest(string Name, string Email, string Password, string PhoneNumber, string Role);