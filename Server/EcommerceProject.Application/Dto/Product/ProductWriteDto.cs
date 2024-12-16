using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Application.Dto.Product;

public record ProductWriteDto(Guid Id, string Title, string Description, decimal Price, uint Quantity, Guid CategoryId);