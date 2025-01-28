namespace Catalog.Application.Dto.Image;

public record ImageDto(string FileName, string ContentType, byte[] Data);