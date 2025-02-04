namespace eShop.Ordering.Domain.Events

/// <summary>
/// Event used when the order stock items are confirmed
/// </summary>
type OrderStatusChangedToStockConfirmedDomainEvent(orderId: int) =
    interface INotification

    member val OrderId = orderId with get
