using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Products.Commands;

public class UpdateCategoryCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

internal class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly IAppDbContext _context;

    public UpdateCategoryCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await _context.Categories.FindAsync(new object[] { request.Id }, ct);
        if (category == null)
            return Result.Failure("Category not found");

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.DisplayOrder = request.DisplayOrder;
        category.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result.Success("Category updated");
    }
}
