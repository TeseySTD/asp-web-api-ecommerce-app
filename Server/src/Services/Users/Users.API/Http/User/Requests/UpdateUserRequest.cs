namespace Users.API.Http.User.Requests;

public record UpdateUserRequest(
    string Name,
    string Email,
    string Password,
    string PhoneNumber,
    string Role
);