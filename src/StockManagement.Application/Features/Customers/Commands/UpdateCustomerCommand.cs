using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Features.Customers.Commands;

public class UpdateCustomerCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}

internal class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result>
{
    private readonly IAppDbContext _context;

    public UpdateCustomerCommandHandler(IAppDbContext context) => _context = context;

    public async Task<Result> Handle(UpdateCustomerCommand request, CancellationToken ct)
    {
        var customer = await _context.Customers.FindAsync(new object[] { request.Id }, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Customer), request.Id);

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.IsActive = request.IsActive;
        customer.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
