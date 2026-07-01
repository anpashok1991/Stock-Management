using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Features.Customers.Commands;

public class CreateCustomerCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

internal class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantContext _tenant;

    public CreateCustomerCommandHandler(IAppDbContext context, ITenantContext tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        var customer = new Customer
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            TenantId = _tenant.TenantId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Customers.AddAsync(customer, ct);
        await _context.SaveChangesAsync(ct);
        return Result<Guid>.Success(customer.Id);
    }
}
