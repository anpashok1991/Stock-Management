using MediatR;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;

namespace StockManagement.Application.Features.Products.Commands;

public class DeleteProductCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}

internal class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IAppDbContext _context;

    public DeleteProductCommandHandler(IAppDbContext context) => _context = context;

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _context.Products.FindAsync(new object[] { request.Id }, ct)
            ?? throw new Common.Exceptions.NotFoundException(nameof(Product), request.Id);

        product.IsDeleted = true;
        product.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
