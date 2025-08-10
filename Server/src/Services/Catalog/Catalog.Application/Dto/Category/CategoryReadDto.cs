namespace Catalog.Application.Dto.Category;

public record CategoryReadDto(Guid Id, string Name, string Description, string[] Images);