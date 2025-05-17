using Catalog.Core.Models.Products.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;

namespace Catalog.Application.UseCases.Products.Commands.DecreaseQuantity;

public class DecreaseQuantityCommandValidator : AbstractValidator<DecreaseQuantityCommand>
{
    public DecreaseQuantityCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .MustBeCreatedWith(ProductId.Create);
        
        RuleFor(x => x.Quantity)
            .NotNull()
            .MustBeCreatedWith(StockQuantity.Create);
    }
}