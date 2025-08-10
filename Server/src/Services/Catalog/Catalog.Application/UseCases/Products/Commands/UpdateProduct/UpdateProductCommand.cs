using Catalog.Application.Dto.Product;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.UpdateProduct;

public record UpdateProductCommand(Guid CurrentSellerId, ProductUpdateDto Value) : ICommand;