using Catalog.Core.Models.Products.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;

namespace Catalog.Application.UseCases.Products.Commands.IncreaseQuantity;

public class IncreaseQuantityCommandValidator : AbstractValidator<IncreaseQuantityCommand>
{
    public IncreaseQuantityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .MustBeCreatedWith(ProductId.Create);
        
        RuleFor(x => x.Quantity)
            .NotNull()
            .MustBeCreatedWith(StockQuantity.Create);
    }
}