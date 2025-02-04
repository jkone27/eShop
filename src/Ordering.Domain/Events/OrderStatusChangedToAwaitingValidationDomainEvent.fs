namespace eShop.Ordering.Domain.Events

open System.Collections.Generic
open eShop.Ordering.Domain.AggregatesModel.OrderAggregate

/// <summary>
/// Event used when the grace period order is confirmed
/// </summary>
type OrderStatusChangedToAwaitingValidationDomainEvent(orderId: int, orderItems: IEnumerable<OrderItem>) =
    interface INotification

    member val OrderId = orderId with get
    member val OrderItems = orderItems with get
