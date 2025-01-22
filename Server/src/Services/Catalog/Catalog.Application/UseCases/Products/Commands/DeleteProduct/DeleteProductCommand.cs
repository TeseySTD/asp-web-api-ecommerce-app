using Catalog.Core.Models.Products.ValueObjects;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.DeleteProduct;

public record DeleteProductCommand(ProductId ProductId) : ICommand;