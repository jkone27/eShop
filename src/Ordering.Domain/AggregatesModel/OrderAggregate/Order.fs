namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate

open System
open System.Linq
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open eShop.Ordering.Domain.Events
open eShop.Ordering.Domain.SeedWork

type Order() =
    inherit Entity()
    interface IAggregateRoot

    [<Required>]
    member val OrderDate = DateTime.MinValue with get, private set

    [<Required>]
    member val Address = Unchecked.defaultof<Address> with get, private set

    member val BuyerId = Nullable<int>() with get, private set

    member val Buyer = Unchecked.defaultof<Buyer> with get

    member val OrderStatus = Unchecked.defaultof<OrderStatus> with get, private set

    member val Description = "" with get, private set

    let mutable _isDraft = false

    let _orderItems = List<OrderItem>()

    member this.OrderItems = _orderItems.AsReadOnly()

    member val PaymentId = Nullable<int>() with get, private set

    static member NewDraft() =
        let order = Order()
        order._isDraft <- true
        order

    new(userId: string, userName: string, address: Address, cardTypeId: int, cardNumber: string, cardSecurityNumber: string, cardHolderName: string, cardExpiration: DateTime, ?buyerId: int, ?paymentMethodId: int) as this =
        Order()
        then
            this.BuyerId <- Nullable(buyerId.GetValueOrDefault())
            this.PaymentId <- Nullable(paymentMethodId.GetValueOrDefault())
            this.OrderStatus <- OrderStatus.Submitted
            this.OrderDate <- DateTime.UtcNow
            this.Address <- address
            this.AddOrderStartedDomainEvent(userId, userName, cardTypeId, cardNumber, cardSecurityNumber, cardHolderName, cardExpiration)

    member this.AddOrderItem(productId: int, productName: string, unitPrice: decimal, discount: decimal, pictureUrl: string, units: int) =
        let existingOrderForProduct = _orderItems.SingleOrDefault(fun o -> o.ProductId = productId)
        if existingOrderForProduct <> null then
            if discount > existingOrderForProduct.Discount then
                existingOrderForProduct.SetNewDiscount(discount)
            existingOrderForProduct.AddUnits(units)
        else
            let orderItem = OrderItem(productId, productName, unitPrice, discount, pictureUrl, units)
            _orderItems.Add(orderItem)

    member this.SetPaymentMethodVerified(buyerId: int, paymentId: int) =
        this.BuyerId <- Nullable(buyerId)
        this.PaymentId <- Nullable(paymentId)

    member this.SetAwaitingValidationStatus() =
        if this.OrderStatus = OrderStatus.Submitted then
            this.AddDomainEvent(OrderStatusChangedToAwaitingValidationDomainEvent(this.Id, _orderItems))
            this.OrderStatus <- OrderStatus.AwaitingValidation

    member this.SetStockConfirmedStatus() =
        if this.OrderStatus = OrderStatus.AwaitingValidation then
            this.AddDomainEvent(OrderStatusChangedToStockConfirmedDomainEvent(this.Id))
            this.OrderStatus <- OrderStatus.StockConfirmed
            this.Description <- "All the items were confirmed with available stock."

    member this.SetPaidStatus() =
        if this.OrderStatus = OrderStatus.StockConfirmed then
            this.AddDomainEvent(OrderStatusChangedToPaidDomainEvent(this.Id, this.OrderItems))
            this.OrderStatus <- OrderStatus.Paid
            this.Description <- "The payment was performed at a simulated \"American Bank checking bank account ending on XX35071\""

    member this.SetShippedStatus() =
        if this.OrderStatus <> OrderStatus.Paid then
            this.StatusChangeException(OrderStatus.Shipped)
        this.OrderStatus <- OrderStatus.Shipped
        this.Description <- "The order was shipped."
        this.AddDomainEvent(OrderShippedDomainEvent(this))

    member this.SetCancelledStatus() =
        if this.OrderStatus = OrderStatus.Paid || this.OrderStatus = OrderStatus.Shipped then
            this.StatusChangeException(OrderStatus.Cancelled)
        this.OrderStatus <- OrderStatus.Cancelled
        this.Description <- "The order was cancelled."
        this.AddDomainEvent(OrderCancelledDomainEvent(this))

    member this.SetCancelledStatusWhenStockIsRejected(orderStockRejectedItems: IEnumerable<int>) =
        if this.OrderStatus = OrderStatus.AwaitingValidation then
            this.OrderStatus <- OrderStatus.Cancelled
            let itemsStockRejectedProductNames = this.OrderItems |> Seq.filter (fun c -> orderStockRejectedItems.Contains(c.ProductId)) |> Seq.map (fun c -> c.ProductName)
            let itemsStockRejectedDescription = String.Join(", ", itemsStockRejectedProductNames)
            this.Description <- $"The product items don't have stock: ({itemsStockRejectedDescription})."

    member private this.AddOrderStartedDomainEvent(userId: string, userName: string, cardTypeId: int, cardNumber: string, cardSecurityNumber: string, cardHolderName: string, cardExpiration: DateTime) =
        let orderStartedDomainEvent = OrderStartedDomainEvent(this, userId, userName, cardTypeId, cardNumber, cardSecurityNumber, cardHolderName, cardExpiration)
        this.AddDomainEvent(orderStartedDomainEvent)

    member private this.StatusChangeException(orderStatusToChange: OrderStatus) =
        raise (OrderingDomainException($"Is not possible to change the order status from {this.OrderStatus} to {orderStatusToChange}."))

    member this.GetTotal() = this.OrderItems |> Seq.sumBy (fun o -> o.Units * o.UnitPrice)
