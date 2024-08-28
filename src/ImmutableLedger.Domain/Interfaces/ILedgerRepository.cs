using ImmutableLedger.Domain.Entities;
using System.Data;

namespace ImmutableLedger.Domain.Interfaces
{
    /// 
    /// Defines the contract for ledger data access operations.
    /// This interface abstracts the data access layer, allowing for different implementations
    /// (e.g., different database systems) without affecting the core business logic.
    /// 
    public interface ILedgerRepository
    {

        Task<IDbTransaction> BeginTransactionAsync();

        /// 
        /// Adds a new entry to the ledger.
        /// 
        /// The ledger entry to add.
        /// A task representing the asynchronous operation.
        Task AddEntryAsync(LedgerEntry entry, IDbTransaction transaction);

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
