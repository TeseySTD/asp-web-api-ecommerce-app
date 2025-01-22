namespace Users.API.Http.Auth.Requests;

public record RegisterUserRequest(string Name, string Email, string Password, string PhoneNumber, string Role);