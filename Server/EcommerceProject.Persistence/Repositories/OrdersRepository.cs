using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Orders;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Persistence.Repositories;

public class OrdersRepository : IOrdersRepository
{
    private readonly StoreDbContext _context;

    public OrdersRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> Get(CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> FindById(OrderId orderId, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<Result> Add(Order order, CancellationToken cancellationToken)
    {
        var resultBuilder = Result.TryFail()
            .CheckError(await _context.Orders.AnyAsync(o => o.Id == order.Id, cancellationToken),
                new Error(nameof(Order), $"Order with id: {order.Id} already exists"))
            .CheckError(!await _context.Users.AnyAsync(u => u.Id == order.UserId, cancellationToken),
                new Error(nameof(User), $"User with id: {order.UserId} not exists"));

        foreach (var item in order.OrderItems)
            resultBuilder.CheckError(!await _context.Products.AnyAsync(p => p.Id == item.ProductId, cancellationToken),
                new Error(nameof(Product), $"Product with id: {item.ProductId} not exists"));


        var result = resultBuilder.Build();
        if (result.IsFailure)
            return result;

        await _context.Orders.AddAsync(order, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Update(Order order, CancellationToken cancellationToken)
    {
        var resultBuilder = Result.TryFail()
            .CheckError(!await _context.Orders.AnyAsync(o => o.Id == order.Id, cancellationToken),
                new Error(nameof(Order), $"Order with id: {order.Id} not exists"))
            .CheckError(!await _context.Users.AnyAsync(u => u.Id == order.UserId, cancellationToken),
                new Error(nameof(User), $"User with id: {order.UserId} not exists"));

        foreach (var item in order.OrderItems)
            resultBuilder.CheckError(!await _context.Products.AnyAsync(p => p.Id == item.ProductId, cancellationToken),
                new Error(nameof(Product), $"Product with id: {item.ProductId} not exists"));


        var result = resultBuilder.Build();
        if (result.IsFailure)
            return result;

        await _context.Orders
            .Where(o => o.Id == order.Id)
            .ExecuteUpdateAsync(p => p
                .SetProperty(pr => pr.OrderItems, order.OrderItems)
                .SetProperty(pr => pr.DestinationAddress, order.DestinationAddress)
                .SetProperty(pr => pr.Payment, order.Payment)
                .SetProperty(pr => pr.UserId, order.UserId));

        return Result.Success();
    }

    public async Task<Result> Delete(Order order, CancellationToken cancellationToken)
    {
        var result = Result.TryFail()
            .CheckError(!await _context.Orders.AnyAsync(o => o.Id == order.Id, cancellationToken),
                new Error(nameof(Order), $"Order with id: {order.Id} does not exist"))
            .Build();

        if (result.IsFailure)
            return result;

        await _context.Orders.Where(o => o.Id == order.Id).ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }


    public Task<bool> Exists(OrderId id, CancellationToken cancellationToken)
    {
        return _context.Orders.AnyAsync(o => o.Id == id, cancellationToken);
    }
}