using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Manufacturing.Queries;

public class GetManufacturingOrderByIdQuery : IRequest<Result<ManufacturingOrderDetailDto>>
{
    public Guid Id { get; set; }
}

public class ManufacturingOrderDetailDto
{
    public Guid Id { get; set; }
    public string ManufacturingNumber { get; set; } = string.Empty;
    public Guid FinishedProductId { get; set; }
    public string FinishedProductName { get; set; } = string.Empty;
    public string? FinishedProductSku { get; set; }
    public Guid? BillOfMaterialId { get; set; }
    public string? BOMName { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ProductionDate { get; set; }
    public decimal TotalMaterialCost { get; set; }
    public decimal AdditionalManufacturingCost { get; set; }
    public decimal LabourCost { get; set; }
    public decimal PackagingCost { get; set; }
    public decimal EstimatedProductCost { get; set; }
    public string? Remarks { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ManufacturingOrderItemDto> Items { get; set; } = new();
}

public class ManufacturingOrderItemDto
{
    public Guid Id { get; set; }
    public Guid RawMaterialId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public string? RawMaterialSku { get; set; }
    public decimal QuantityRequired { get; set; }
    public decimal QuantityConsumed { get; set; }
    public string Unit { get; set; } = "Piece";
    public decimal WastagePercentage { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public int AvailableStock { get; set; }
    public bool IsOptional { get; set; }
    public string? Remarks { get; set; }
}

internal class GetManufacturingOrderByIdQueryHandler : IRequestHandler<GetManufacturingOrderByIdQuery, Result<ManufacturingOrderDetailDto>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public GetManufacturingOrderByIdQueryHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<ManufacturingOrderDetailDto>> Handle(GetManufacturingOrderByIdQuery request, CancellationToken ct)
    {
        var order = await _context.ManufacturingOrders
            .Include(m => m.FinishedProduct)
            .Include(m => m.BillOfMaterial)
            .Include(m => m.Items).ThenInclude(mi => mi.RawMaterial)
            .FirstOrDefaultAsync(m => m.Id == request.Id, ct);

        if (order == null)
            return Result<ManufacturingOrderDetailDto>.Failure("Manufacturing order not found");

        var dto = new ManufacturingOrderDetailDto
        {
            Id = order.Id,
            ManufacturingNumber = order.ManufacturingNumber,
            FinishedProductId = order.FinishedProductId,
            FinishedProductName = order.FinishedProduct?.Name ?? string.Empty,
            FinishedProductSku = order.FinishedProduct?.Sku,
            BillOfMaterialId = order.BillOfMaterialId,
            BOMName = order.BillOfMaterial?.Name,
            Quantity = order.Quantity,
            Status = order.Status.ToString(),
            ProductionDate = order.ProductionDate,
            TotalMaterialCost = order.TotalMaterialCost,
            AdditionalManufacturingCost = order.AdditionalManufacturingCost,
            LabourCost = order.LabourCost,
            PackagingCost = order.PackagingCost,
            EstimatedProductCost = order.EstimatedProductCost,
            Remarks = order.Remarks,
            CreatedBy = order.CreatedBy,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(mi => new ManufacturingOrderItemDto
            {
                Id = mi.Id,
                RawMaterialId = mi.RawMaterialId,
                RawMaterialName = mi.RawMaterial?.Name ?? string.Empty,
                RawMaterialSku = mi.RawMaterial?.Sku,
                QuantityRequired = mi.QuantityRequired,
                QuantityConsumed = mi.QuantityConsumed,
                Unit = mi.Unit,
                WastagePercentage = mi.WastagePercentage,
                UnitCost = mi.UnitCost,
                TotalCost = mi.TotalCost,
                AvailableStock = mi.AvailableStock,
                IsOptional = mi.IsOptional,
                Remarks = mi.Remarks
            }).ToList()
        };

        return Result<ManufacturingOrderDetailDto>.Success(dto);
    }
}
