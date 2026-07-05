using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities.Manufacturing;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Manufacturing.Commands;

public class CreateBOMCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public Guid FinishedProductId { get; set; }
    public string VersionNumber { get; set; } = "1.0";
    public BOMStatus Status { get; set; } = BOMStatus.Draft;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public List<BOMItemRequest> Items { get; set; } = new();
}

public class BOMItemRequest
{
    public Guid RawMaterialId { get; set; }
    public decimal QuantityRequired { get; set; }
    public string Unit { get; set; } = "Piece";
    public decimal WastagePercentage { get; set; }
    public bool IsOptional { get; set; }
    public string? Remarks { get; set; }
}

internal class CreateBOMCommandHandler : IRequestHandler<CreateBOMCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public CreateBOMCommandHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateBOMCommand request, CancellationToken ct)
    {
        var bom = new BillOfMaterial
        {
            Name = request.Name,
            FinishedProductId = request.FinishedProductId,
            VersionNumber = request.VersionNumber,
            Status = request.Status,
            Description = request.Description,
            Notes = request.Notes,
            CreatedBy = _tenant.TenantName,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
                bom.Items.Add(new BillOfMaterialItem
                {
                    RawMaterialId = item.RawMaterialId,
                    QuantityRequired = item.QuantityRequired,
                    Unit = item.Unit,
                    WastagePercentage = item.WastagePercentage,
                    IsOptional = item.IsOptional,
                    Remarks = item.Remarks
                });
        }

        await _context.BillOfMaterials.AddAsync(bom, ct);
        await _context.SaveChangesAsync(ct);

        return Result<Guid>.Success(bom.Id);
    }
}
