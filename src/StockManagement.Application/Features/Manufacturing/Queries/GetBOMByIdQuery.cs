using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Manufacturing.Queries;

public class GetBOMByIdQuery : IRequest<Result<BOMDetailDto>>
{
    public Guid Id { get; set; }
}

public class BOMDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid FinishedProductId { get; set; }
    public string FinishedProductName { get; set; } = string.Empty;
    public string? FinishedProductSku { get; set; }
    public string VersionNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BOMItemDetailDto> Items { get; set; } = new();
}

public class BOMItemDetailDto
{
    public Guid Id { get; set; }
    public Guid RawMaterialId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public string? RawMaterialSku { get; set; }
    public decimal QuantityRequired { get; set; }
    public string Unit { get; set; } = "Piece";
    public decimal WastagePercentage { get; set; }
    public bool IsOptional { get; set; }
    public string? Remarks { get; set; }
    public int AvailableStock { get; set; }
}

internal class GetBOMByIdQueryHandler : IRequestHandler<GetBOMByIdQuery, Result<BOMDetailDto>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetBOMByIdQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<BOMDetailDto>> Handle(GetBOMByIdQuery request, CancellationToken ct)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.FinishedProduct)
            .Include(b => b.Items).ThenInclude(bi => bi.RawMaterial)
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

        if (bom == null)
            return Result<BOMDetailDto>.Failure("Bill of Materials not found");

        var dto = new BOMDetailDto
        {
            Id = bom.Id,
            Name = bom.Name,
            FinishedProductId = bom.FinishedProductId,
            FinishedProductName = bom.FinishedProduct?.Name ?? string.Empty,
            FinishedProductSku = bom.FinishedProduct?.Sku,
            VersionNumber = bom.VersionNumber,
            Status = bom.Status.ToString(),
            Description = bom.Description,
            Notes = bom.Notes,
            CreatedBy = bom.CreatedBy,
            CreatedAt = bom.CreatedAt,
            Items = bom.Items.Select(bi => new BOMItemDetailDto
            {
                Id = bi.Id,
                RawMaterialId = bi.RawMaterialId,
                RawMaterialName = bi.RawMaterial?.Name ?? string.Empty,
                RawMaterialSku = bi.RawMaterial?.Sku,
                QuantityRequired = bi.QuantityRequired,
                Unit = bi.Unit,
                WastagePercentage = bi.WastagePercentage,
                IsOptional = bi.IsOptional,
                Remarks = bi.Remarks,
                AvailableStock = _context.StockItems
                    .Where(s => s.ProductId == bi.RawMaterialId)
                    .Select(s => s.Quantity)
                    .FirstOrDefault()
            }).ToList()
        };

        return Result<BOMDetailDto>.Success(dto);
    }
}
