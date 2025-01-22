namespace Users.Application.Dto.User;

public record UserWriteDto(string Name, string Email, string Password, string PhoneNumber, string Role);