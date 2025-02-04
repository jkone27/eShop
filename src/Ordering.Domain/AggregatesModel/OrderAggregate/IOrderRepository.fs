namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate

// This is just the RepositoryContracts or Interface defined at the Domain Layer
// as requisite for the Order Aggregate

type IOrderRepository =
    abstract member Add: order: Order -> Order
    abstract member Update: order: Order -> unit
    abstract member GetAsync: orderId: int -> Task<Order>
