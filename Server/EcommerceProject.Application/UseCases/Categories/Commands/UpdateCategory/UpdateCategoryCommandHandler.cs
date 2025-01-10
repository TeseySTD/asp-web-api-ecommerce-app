using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var updatedCategoryId = CategoryId.Create(request.Value.Id).Value;
        var updatedCategory = Category.Create(
            id: updatedCategoryId,
            name: CategoryName.Create(request.Value.Name).Value,
            description: CategoryDescription.Create(request.Value.Description).Value
        );
        
        return Update(updatedCategory, cancellationToken);
    }
    
    public async Task<Result> Update( Category category, CancellationToken cancellationToken = default)
    {
        if(!await _context.Categories.AnyAsync(p => p.Id == category.Id, cancellationToken))
            return Error.NotFound;

        try
        {
            var categoryToUpdate = await _context.Categories.FindAsync(category.Id, cancellationToken);
            categoryToUpdate!.Update(
                name: category.Name,
                description: category.Description);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return Result.Success();
        }
        catch (Exception e)
        {
            return new Error(e.Message, e.StackTrace ?? string.Empty);
        }
        
    }
}