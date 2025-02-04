namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate

open System
open System.ComponentModel.DataAnnotations

type OrderItem(productId: int, productName: string, unitPrice: decimal, discount: decimal, pictureUrl: string, units: int) =
    inherit Entity()

    do
        if units <= 0 then
            raise (OrderingDomainException("Invalid number of units"))
        if (unitPrice * decimal units) < discount then
            raise (OrderingDomainException("The total of order item is lower than applied discount"))

    member val ProductId = productId with get, private set
    member val ProductName = productName with get, private set
    member val UnitPrice = unitPrice with get, private set
    member val Discount = discount with get, private set
    member val Units = units with get, private set
    member val PictureUrl = pictureUrl with get, private set

    new() = OrderItem(0, "", 0M, 0M, "", 1)

    member this.SetNewDiscount(discount: decimal) =
        if discount < 0M then
            raise (OrderingDomainException("Discount is not valid"))
        this.Discount <- discount

    member this.AddUnits(units: int) =
        if units < 0 then
            raise (OrderingDomainException("Invalid units"))
        this.Units <- this.Units + units
