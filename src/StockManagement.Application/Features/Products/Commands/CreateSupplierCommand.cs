using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Features.Products.Commands;

public class CreateSupplierCommand : IRequest<Result<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? GstNumber { get; set; }
}

internal class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;

    public CreateSupplierCommandHandler(IAppDbContext context) => _context = context;

    public async Task<Result<Guid>> Handle(CreateSupplierCommand request, CancellationToken ct)
    {
        var supplier = new Supplier
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            GstNumber = request.GstNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync(ct);
        return Result<Guid>.Success(supplier.Id);
    }
}
