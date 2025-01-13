using Catalog.Application.Dto.Product;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(ProductWriteDto Value) : ICommand;


