namespace eShop.Ordering.Domain.Exceptions

open System

/// <summary>
/// Exception type for domain exceptions
/// </summary>
type OrderingDomainException() =
    inherit Exception()

    new(message: string) =
        OrderingDomainException()
        then
            base.Message <- message

    new(message: string, innerException: Exception) =
        OrderingDomainException()
        then
            base.Message <- message
            base.InnerException <- innerException
