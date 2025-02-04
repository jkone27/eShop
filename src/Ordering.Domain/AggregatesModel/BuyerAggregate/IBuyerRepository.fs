namespace eShop.Ordering.Domain.AggregatesModel.BuyerAggregate

open System.Threading.Tasks

// This is just the RepositoryContracts or Interface defined at the Domain Layer
// as requisite for the Buyer Aggregate

type IBuyerRepository =
    abstract member Add: buyer: Buyer -> Buyer
    abstract member Update: buyer: Buyer -> Buyer
    abstract member FindAsync: buyerIdentityGuid: string -> Task<Buyer>
    abstract member FindByIdAsync: id: int -> Task<Buyer>
