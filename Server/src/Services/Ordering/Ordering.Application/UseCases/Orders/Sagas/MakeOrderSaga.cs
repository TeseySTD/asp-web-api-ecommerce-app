using MassTransit;
using Shared.Messaging.Events.Order;
using Shared.Messaging.Messages.Order;

namespace Ordering.Application.UseCases.Orders.Sagas;

public class MakeOrderSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    public List<ProductWithQuantityDto> ProductWithQuantityDtos { get; set; }
}

public class MakeOrderSaga : MassTransitStateMachine<MakeOrderSagaState>
{
    public const string CheckingCustomerFailedReason = "Checking customer failed.";

    public MakeOrderSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderMade, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => ReservedProducts, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => ReservationProductsFailed, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => CheckedCustomer, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => CheckingCustomerFailed, x => x.CorrelateById(m => m.Message.OrderId));

        Initially(
            When(OrderMade)
                .Then(context => context.Saga.ProductWithQuantityDtos = context.Message.Products)
                .Publish(context => new
                    CheckCustomerMessage(
                        OrderId: context.Message.OrderId,
                        CustomerId: context.Message.CustomerId
                    )
                )
                .TransitionTo(CheckingCustomer)
        );

        During(CheckingCustomer,
            When(CheckedCustomer)
                .Publish(context => new
                    ReserveProductsMessage(
                        OrderId: context.Message.OrderId,
                        Products: context.Saga.ProductWithQuantityDtos
                    )
                )
                .TransitionTo(ReservingProducts),
            When(CheckingCustomerFailed)
                .Publish(context => new
                    CanceledOrderEvent(
                        OrderId: context.Message.OrderId,
                        Reason: CheckingCustomerFailedReason 
                    )
                )
                .Finalize()
        );

        During(ReservingProducts,
            When(ReservedProducts)
                .Publish(context => new
                    ApprovedOrderEvent(
                        OrderId: context.Message.OrderId,
                        OrderItemsDtos: context.Message.OrderItemsDtos
                    )
                )
                .Finalize(),
            When(ReservationProductsFailed)
                .Publish(context => new
                    CanceledOrderEvent(
                        OrderId: context.Message.OrderId,
                        Reason: context.Message.Reason
                    )
                )
                .Finalize()
        );
    }

    public State ReservingProducts { get; private set; }
    public State CheckingCustomer { get; private set; }


    public Event<OrderMadeEvent> OrderMade { get; private set; }
    public Event<CheckedCustomerEvent> CheckedCustomer { get; private set; }
    public Event<CheckingCustomerFailedEvent> CheckingCustomerFailed { get; private set; }
    public Event<ReservedProductsEvent> ReservedProducts { get; private set; }
    public Event<ReservationProductsFailedEvent> ReservationProductsFailed { get; private set; }
}