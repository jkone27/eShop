namespace eShop.Ordering.Infrastructure

open System
open System.Linq
open System.Threading
open System.Threading.Tasks
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Storage
open eShop.IntegrationEventLogEF
open eShop.Ordering.Domain.AggregatesModel.BuyerAggregate
open eShop.Ordering.Domain.AggregatesModel.OrderAggregate
open eShop.Ordering.Domain.SeedWork
open MediatR

/// <remarks>
/// Add migrations using the following command inside the 'Ordering.Infrastructure' project directory:
///
/// dotnet ef migrations add --startup-project Ordering.API --context OrderingContext [migration-name]
/// </remarks>
type OrderingContext(options: DbContextOptions<OrderingContext>, mediator: IMediator) as this =
    inherit DbContext(options)

    let mutable _currentTransaction: IDbContextTransaction = null

    do
        if isNull mediator then raise (ArgumentNullException(nameof(mediator)))

    member val Orders = this.Set<Order>() with get, set
    member val OrderItems = this.Set<OrderItem>() with get, set
    member val Payments = this.Set<PaymentMethod>() with get, set
    member val Buyers = this.Set<Buyer>() with get, set
    member val CardTypes = this.Set<CardType>() with get, set

    member this.GetCurrentTransaction() = _currentTransaction

    member this.HasActiveTransaction = not (isNull _currentTransaction)

    override this.OnModelCreating(modelBuilder: ModelBuilder) =
        modelBuilder.HasDefaultSchema("ordering")
        modelBuilder.ApplyConfiguration(ClientRequestEntityTypeConfiguration())
        modelBuilder.ApplyConfiguration(PaymentMethodEntityTypeConfiguration())
        modelBuilder.ApplyConfiguration(OrderEntityTypeConfiguration())
        modelBuilder.ApplyConfiguration(OrderItemEntityTypeConfiguration())
        modelBuilder.ApplyConfiguration(CardTypeEntityTypeConfiguration())
        modelBuilder.ApplyConfiguration(BuyerEntityTypeConfiguration())
        modelBuilder.UseIntegrationEventLogs()

    member this.SaveEntitiesAsync(cancellationToken: CancellationToken) =
        task {
            // Dispatch Domain Events collection. 
            // Choices:
            // A) Right BEFORE committing data (EF SaveChanges) into the DB will make a single transaction including  
            // side effects from the domain event handlers which are using the same DbContext with "InstancePerLifetimeScope" or "scoped" lifetime
            // B) Right AFTER committing data (EF SaveChanges) into the DB will make multiple transactions. 
            // You will need to handle eventual consistency and compensatory actions in case of failures in any of the Handlers. 
            do! mediator.DispatchDomainEventsAsync(this)

            // After executing this line all the changes (from the Command Handler and Domain Event Handlers) 
            // performed through the DbContext will be committed
            let! _ = base.SaveChangesAsync(cancellationToken)

            return true
        }

    member this.BeginTransactionAsync() =
        task {
            if not (isNull _currentTransaction) then return null
            _currentTransaction <- await this.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted)
            return _currentTransaction
        }

    member this.CommitTransactionAsync(transaction: IDbContextTransaction) =
        task {
            if isNull transaction then raise (ArgumentNullException(nameof(transaction)))
            if transaction <> _currentTransaction then raise (InvalidOperationException($"Transaction {transaction.TransactionId} is not current"))

            try
                do! this.SaveChangesAsync()
                do! transaction.CommitAsync()
            with
            | _ ->
                this.RollbackTransaction()
                reraise()
            finally
                if this.HasActiveTransaction then
                    _currentTransaction.Dispose()
                    _currentTransaction <- null
        }

    member this.RollbackTransaction() =
        try
            _currentTransaction?.Rollback()
        finally
            if this.HasActiveTransaction then
                _currentTransaction.Dispose()
                _currentTransaction <- null
