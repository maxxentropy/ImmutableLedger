using ImmutableLedger.Domain.Entities;

namespace ImmutableLedger.Application.Interfaces
{
    /// 
    /// Defines the contract for ledger operations at the application level.
    /// This interface abstracts the business logic for ledger operations,
    /// allowing for easier testing and flexibility in implementation.
    /// 
    public interface ILedgerService
    {
        /// 
        /// Creates a new ledger entry.
        /// 
        /// The source account.
        /// The destination account.
        /// The transaction amount.
        /// A description of the transaction.
        /// The region associated with this entry.
        /// A task representing the asynchronous operation.
        Task CreateEntryAsync(string accountFrom, string accountTo, decimal amount, string description, string region);

        /// 
        /// Retrieves all ledger entries for a specific region.
        /// 
        /// The region to query.
        /// A collection of ledger entries for the specified region.
        Task<IEnumerable<LedgerEntry>> GetEntriesByRegionAsync(string region);

        /// 
        /// Retrieves all ledger entries involving a specific account.
        /// 
        /// The account to query.
        /// A collection of ledger entries involving the specified account.
        Task<IEnumerable<LedgerEntry>> GetEntriesByAccountAsync(string account);
    }
}
