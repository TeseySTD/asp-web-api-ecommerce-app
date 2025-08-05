using Catalog.Core.Models.Products.ValueObjects;
using Shared.Core.Auth;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.DeleteProduct;

public record DeleteProductCommand(ProductId ProductId, SellerId SellerId, UserRole Role) : ICommand;