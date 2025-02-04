namespace eShop.Ordering.Domain.Events

open System

type BuyerAndPaymentMethodVerifiedDomainEvent(buyer: Buyer, payment: PaymentMethod, orderId: int) =
    interface INotification

    member val Buyer = buyer with get, private set
    member val Payment = payment with get, private set
    member val OrderId = orderId with get, private set
