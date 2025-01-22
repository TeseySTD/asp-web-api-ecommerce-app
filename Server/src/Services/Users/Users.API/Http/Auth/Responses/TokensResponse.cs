namespace Users.API.Http.Auth.Responses;

public record TokensResponse(string AccessToken, string RefreshToken);