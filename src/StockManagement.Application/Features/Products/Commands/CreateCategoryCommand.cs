using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Features.Products.Commands;

public class CreateCategoryCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
}

internal class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;

    public CreateCategoryCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(category.Id);
    }
}
