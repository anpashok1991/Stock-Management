using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities.Manufacturing;
using StockManagement.Domain.Enums;

namespace StockManagement.Application.Features.Manufacturing.Commands;

public class UpdateBOMCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid FinishedProductId { get; set; }
    public string VersionNumber { get; set; } = "1.0";
    public BOMStatus Status { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public List<BOMItemRequest> Items { get; set; } = new();
}

internal class UpdateBOMCommandHandler : IRequestHandler<UpdateBOMCommand, Result>
{
    private readonly IAppDbContext _context;

    public UpdateBOMCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateBOMCommand request, CancellationToken ct)
    {
        // Load BOM without items to avoid tracking conflicts on item deletion
        var bom = await _context.BillOfMaterials
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

        if (bom == null)
            return Result.Failure("Bill of Materials not found");

        // Delete existing items
        var oldItems = await _context.BillOfMaterialItems
            .Where(bi => bi.BillOfMaterialId == request.Id)
            .ToListAsync(ct);

        _context.BillOfMaterialItems.RemoveRange(oldItems);

        bom.Name = request.Name;
        bom.FinishedProductId = request.FinishedProductId;
        bom.VersionNumber = request.VersionNumber;
        bom.Status = request.Status;
        bom.Description = request.Description;
        bom.Notes = request.Notes;
        bom.ModifiedAt = DateTime.UtcNow;

        foreach (var item in request.Items)
        {
            _context.BillOfMaterialItems.Add(new BillOfMaterialItem
            {
                BillOfMaterialId = bom.Id,
                RawMaterialId = item.RawMaterialId,
                QuantityRequired = item.QuantityRequired,
                Unit = item.Unit,
                WastagePercentage = item.WastagePercentage,
                IsOptional = item.IsOptional,
                Remarks = item.Remarks
            });
        }

        await _context.SaveChangesAsync(ct);

        return Result.Success("Bill of Materials updated successfully");
    }
}
