namespace eShop.Ordering.Domain.AggregatesModel.OrderAggregate

open eShop.Ordering.Domain.SeedWork

type Address(street: string, city: string, state: string, country: string, zipcode: string) =
    inherit ValueObject()

    member val Street = street with get, private set
    member val City = city with get, private set
    member val State = state with get, private set
    member val Country = country with get, private set
    member val ZipCode = zipcode with get, private set

    new() = Address("", "", "", "", "")

    override this.GetEqualityComponents() =
        seq {
            yield this.Street
            yield this.City
            yield this.State
            yield this.Country
            yield this.ZipCode
        }
