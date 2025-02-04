namespace eShop.Ordering.Domain.AggregatesModel.BuyerAggregate

open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open eShop.Ordering.Domain.Events

type Buyer(identity: string, name: string) =
    inherit Entity()
    do
        if String.IsNullOrWhiteSpace(identity) then raise (ArgumentNullException(nameof(identity)))
        if String.IsNullOrWhiteSpace(name) then raise (ArgumentNullException(nameof(name)))

    [<Required>]
    member val IdentityGuid = identity with get, private set

    member val Name = name with get, private set

    let mutable _paymentMethods = List<PaymentMethod>()

    member this.PaymentMethods = _paymentMethods.AsReadOnly()

    new() = Buyer("", "")

    member this.VerifyOrAddPaymentMethod(cardTypeId: int, alias: string, cardNumber: string, securityNumber: string, cardHolderName: string, expiration: DateTime, orderId: int) =
        let existingPayment = _paymentMethods |> List.tryFind (fun p -> p.IsEqualTo(cardTypeId, cardNumber, expiration))
        match existingPayment with
        | Some payment ->
            this.AddDomainEvent(BuyerAndPaymentMethodVerifiedDomainEvent(this, payment, orderId))
            payment
        | None ->
            let payment = PaymentMethod(cardTypeId, alias, cardNumber, securityNumber, cardHolderName, expiration)
            _paymentMethods <- payment :: _paymentMethods
            this.AddDomainEvent(BuyerAndPaymentMethodVerifiedDomainEvent(this, payment, orderId))
            payment
