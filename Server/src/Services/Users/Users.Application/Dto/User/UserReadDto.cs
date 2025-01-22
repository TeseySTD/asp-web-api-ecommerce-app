namespace Users.Application.Dto.User;

public record UserReadDto(Guid Id, string Name, string Email, string Password, string PhoneNumber, string Role);