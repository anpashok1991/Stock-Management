using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace StockManagement.Application.Features.Products.Commands;

public class DeleteSupplierCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}

internal class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, Result>
{
    private readonly IAppDbContext _context;

    public DeleteSupplierCommandHandler(IAppDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteSupplierCommand request, CancellationToken ct)
    {
        var supplier = await _context.Suppliers
            .Include(s => s.Products)
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (supplier == null) return Result.Failure("Supplier not found");
        if (supplier.Products.Any(p => !p.IsDeleted))
            return Result.Failure("Cannot delete supplier with active products");

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync(ct);
        return Result.Success("Supplier deleted");
    }
}
