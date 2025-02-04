namespace eShop.Ordering.Domain.Events

open System.Collections.Generic
open eShop.Ordering.Domain.AggregatesModel.OrderAggregate

/// <summary>
/// Event used when the order is paid
/// </summary>
type OrderStatusChangedToPaidDomainEvent(orderId: int, orderItems: IEnumerable<OrderItem>) =
    interface INotification

    member val OrderId = orderId with get
    member val OrderItems = orderItems with get
