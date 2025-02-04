namespace eShop.Ordering.Domain.Events

open MediatR
open eShop.Ordering.Domain.AggregatesModel.OrderAggregate

type OrderCancelledDomainEvent(order: Order) =
    interface INotification
    member val Order = order with get
