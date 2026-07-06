using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Products.Queries;

public class GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>
{
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public int ProductCount { get; set; }
}

internal class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetCategoriesQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var query = _context.Categories.AsQueryable();
        if (_tenant.TenantId.HasValue)
            query = query.Where(c => c.TenantId == _tenant.TenantId.Value);

        var items = await query
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                DisplayOrder = c.DisplayOrder,
                ProductCount = c.Products.Count(p => !p.IsDeleted)
            })
            .ToListAsync(ct);

        return Result<List<CategoryDto>>.Success(items);
    }
}
