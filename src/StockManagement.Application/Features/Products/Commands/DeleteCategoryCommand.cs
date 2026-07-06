using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace StockManagement.Application.Features.Products.Commands;

public class DeleteCategoryCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}

internal class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly IAppDbContext _context;

    public DeleteCategoryCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

        if (category == null)
            return Result.Failure("Category not found");

        if (category.Products.Any(p => !p.IsDeleted))
            return Result.Failure("Cannot delete category with active products. Reassign products first.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(ct);
        return Result.Success("Category deleted");
    }
}
