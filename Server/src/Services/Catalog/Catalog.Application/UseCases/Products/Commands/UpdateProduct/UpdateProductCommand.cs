using Catalog.Application.Dto.Product;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.UpdateProduct;

public record UpdateProductCommand(ProductUpdateDto Value) : ICommand;