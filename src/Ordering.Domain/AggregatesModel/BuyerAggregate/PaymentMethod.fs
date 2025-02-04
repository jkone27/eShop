namespace eShop.Ordering.Domain.AggregatesModel.BuyerAggregate

open System
open System.ComponentModel.DataAnnotations

type PaymentMethod(cardTypeId: int, alias: string, cardNumber: string, securityNumber: string, cardHolderName: string, expiration: DateTime) =
    inherit Entity()
    do
        if String.IsNullOrWhiteSpace(cardNumber) then raise (OrderingDomainException(nameof(cardNumber)))
        if String.IsNullOrWhiteSpace(securityNumber) then raise (OrderingDomainException(nameof(securityNumber)))
        if String.IsNullOrWhiteSpace(cardHolderName) then raise (OrderingDomainException(nameof(cardHolderName)))
        if expiration < DateTime.UtcNow then raise (OrderingDomainException(nameof(expiration)))

    [<Required>]
    member val Alias = alias with get, private set
    [<Required>]
    member val CardNumber = cardNumber with get, private set
    member val SecurityNumber = securityNumber with get, private set
    [<Required>]
    member val CardHolderName = cardHolderName with get, private set
    member val Expiration = expiration with get, private set

    member val CardTypeId = cardTypeId with get, private set
    member val CardType = Unchecked.defaultof<CardType> with get, private set

    new() = PaymentMethod(0, "", "", "", "", DateTime.MinValue)

    member this.IsEqualTo(cardTypeId: int, cardNumber: string, expiration: DateTime) =
        this.CardTypeId = cardTypeId && this.CardNumber = cardNumber && this.Expiration = expiration
