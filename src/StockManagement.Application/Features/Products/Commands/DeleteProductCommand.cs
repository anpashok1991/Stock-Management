using MediatR;
using Microsoft.EntityFrameworkCore;
using StockManagement.Application.Common.Interfaces;
using StockManagement.Application.Common.Models;
using StockManagement.Domain.Entities;
using StockManagement.Domain.Entities.Inventory;

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

        var stockItems = await _context.StockItems
            .Where(s => s.ProductId == request.Id)
            .ToListAsync(ct);

        if (stockItems.Any())
            _context.StockItems.RemoveRange(stockItems);

        product.IsDeleted = true;
        product.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result.Success();
    }
}
