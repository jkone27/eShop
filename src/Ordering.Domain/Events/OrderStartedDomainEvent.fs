namespace eShop.Ordering.Domain.Events

open System

/// <summary>
/// Event used when an order is created
/// </summary>
type OrderStartedDomainEvent(order: Order, userId: string, userName: string, cardTypeId: int, cardNumber: string, cardSecurityNumber: string, cardHolderName: string, cardExpiration: DateTime) =
    interface INotification

    member val Order = order with get, private set
    member val UserId = userId with get, private set
    member val UserName = userName with get, private set
    member val CardTypeId = cardTypeId with get, private set
    member val CardNumber = cardNumber with get, private set
    member val CardSecurityNumber = cardSecurityNumber with get, private set
    member val CardHolderName = cardHolderName with get, private set
    member val CardExpiration = cardExpiration with get, private set
