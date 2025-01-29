namespace Shared.Core.API;

public record PaginationRequest(int PageIndex = 0, int PageSize = 10);