using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;

namespace StockManagement.Application.Features.Products.Commands;

public class UpdateSupplierCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? GstNumber { get; set; }
    public bool IsActive { get; set; }
}

internal class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, Result>
{
    private readonly IAppDbContext _context;

    public UpdateSupplierCommandHandler(IAppDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateSupplierCommand request, CancellationToken ct)
    {
        var supplier = await _context.Suppliers.FindAsync(new object[] { request.Id }, ct);
        if (supplier == null) return Result.Failure("Supplier not found");

        supplier.Name = request.Name;
        supplier.ContactPerson = request.ContactPerson;
        supplier.Email = request.Email;
        supplier.Phone = request.Phone;
        supplier.Address = request.Address;
        supplier.GstNumber = request.GstNumber;
        supplier.IsActive = request.IsActive;
        supplier.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result.Success("Supplier updated");
    }
}
