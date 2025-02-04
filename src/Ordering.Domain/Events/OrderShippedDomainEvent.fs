namespace eShop.Ordering.Domain.Events

type OrderShippedDomainEvent(order: Order) =
    interface INotification
    member val Order = order with get
