using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Orders;
using EcommerceProject.Core.Models.Orders.ValueObjects;

namespace EcommerceProject.Application.Common.Interfaces.Repositories;

public interface IOrdersRepository
{
    Task<IEnumerable<Order>> Get(CancellationToken cancellationToken);
    Task<Order?> FindById(OrderId orderId, CancellationToken cancellationToken);
    Task<Result> Add(Order order, CancellationToken cancellationToken);
    Task<Result> Update(Order order, CancellationToken cancellationToken);
    Task<Result> Delete(Order order, CancellationToken cancellationToken);
    Task<bool> Exists(OrderId id, CancellationToken cancellationToken);
}