using Catalog.Application.Common.Interfaces;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.DeleteProductImage;

public record DeleteProductImageCommand(Guid ProductId, Guid ImageId) : ICommand;