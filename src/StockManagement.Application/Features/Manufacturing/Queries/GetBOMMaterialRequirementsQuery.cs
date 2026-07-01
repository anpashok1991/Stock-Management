using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Manufacturing.Queries;

public class GetBOMMaterialRequirementsQuery : IRequest<Result<List<BOMMaterialRequirementDto>>>
{
    public Guid BOMId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class BOMMaterialRequirementDto
{
    public Guid RawMaterialId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public string? RawMaterialSku { get; set; }
    public decimal QuantityRequired { get; set; }
    public decimal QuantityWithWastage { get; set; }
    public string Unit { get; set; } = "Piece";
    public decimal WastagePercentage { get; set; }
    public int AvailableStock { get; set; }
    public bool IsInsufficient { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public bool IsOptional { get; set; }
    public string? Remarks { get; set; }
}

internal class GetBOMMaterialRequirementsQueryHandler : IRequestHandler<GetBOMMaterialRequirementsQuery, Result<List<BOMMaterialRequirementDto>>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetBOMMaterialRequirementsQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<List<BOMMaterialRequirementDto>>> Handle(GetBOMMaterialRequirementsQuery request, CancellationToken ct)
    {
        var bom = await _context.BillOfMaterials
            .Include(b => b.Items).ThenInclude(bi => bi.RawMaterial)
            .FirstOrDefaultAsync(b => b.Id == request.BOMId, ct);

        if (bom == null)
            return Result<List<BOMMaterialRequirementDto>>.Failure("Bill of Materials not found");

        var tenantId = _tenant.TenantId ?? Guid.Empty;

        var requirements = bom.Items.Select(bi =>
        {
            var quantityRequired = bi.QuantityRequired * request.Quantity;
            var wastageQty = quantityRequired * (bi.WastagePercentage / 100m);
            var quantityWithWastage = quantityRequired + wastageQty;

            var availableStock = _context.StockItems
                .Where(s => s.ProductId == bi.RawMaterialId && s.TenantId == tenantId)
                .Select(s => s.Quantity)
                .FirstOrDefault();

            var unitCost = bi.RawMaterial?.CostPrice ?? 0;
            var totalCost = quantityWithWastage * unitCost;

            return new BOMMaterialRequirementDto
            {
                RawMaterialId = bi.RawMaterialId,
                RawMaterialName = bi.RawMaterial?.Name ?? string.Empty,
                RawMaterialSku = bi.RawMaterial?.Sku,
                QuantityRequired = quantityRequired,
                QuantityWithWastage = quantityWithWastage,
                Unit = bi.Unit,
                WastagePercentage = bi.WastagePercentage,
                AvailableStock = availableStock,
                IsInsufficient = availableStock < (int)quantityWithWastage,
                UnitCost = unitCost,
                TotalCost = totalCost,
                IsOptional = bi.IsOptional,
                Remarks = bi.Remarks
            };
        }).ToList();

        return Result<List<BOMMaterialRequirementDto>>.Success(requirements);
    }
}
