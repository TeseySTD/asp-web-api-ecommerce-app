namespace Users.Application.Dto.User;

public record UserUpdateDto(string Name, string Email, string Password, string PhoneNumber, string Role);