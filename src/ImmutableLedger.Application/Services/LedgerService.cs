using ImmutableLedger.Application.Interfaces;
using ImmutableLedger.Domain.Entities;
using ImmutableLedger.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ImmutableLedger.Application.Services
{
    /// 
    /// Implements the ILedgerService interface, providing the core application logic for ledger operations.
    /// This service coordinates between the domain entities and the data access layer.
    /// 
    public class LedgerService : ILedgerService
    {
        private readonly ILedgerRepository _repository;
        private readonly ILogger<LedgerService> _logger;

        public LedgerService(ILedgerRepository repository, ILogger<LedgerService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a ledger entry using the provided account details and persists it to the repository.
        /// </summary>
        /// <param name="accountFrom">The source account for the ledger entry.</param>
        /// <param name="accountTo">The destination account for the ledger entry.</param>
        /// <param name="amount">The amount to be transferred between accounts.</param>
        /// <param name="description">A description of the ledger entry.</param>
        /// <param name="region">The region associated with the ledger entry.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if any of the string parameters are null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the amount is less than or equal to zero.</exception>
        /// <exception cref="Exception">Thrown if there is an error adding the entry to the repository.</exception>
        public async Task CreateEntryAsync(string accountFrom, string accountTo, decimal amount, string description, string region)
        {
            if (string.IsNullOrWhiteSpace(accountFrom))
            {
                throw new ArgumentException("AccountFrom cannot be null or empty", nameof(accountFrom));
            }

            if (string.IsNullOrWhiteSpace(accountTo))
            {
                throw new ArgumentException("AccountTo cannot be null or empty", nameof(accountTo));
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description cannot be null or empty", nameof(description));
            }

            if (string.IsNullOrWhiteSpace(region))
            {
                throw new ArgumentException("Region cannot be null or empty", nameof(region));
            }

            var entry = new LedgerEntry(accountFrom, accountTo, amount, description, region);

            using (var transaction = await _repository.BeginTransactionAsync())
            {
                try
                {
                    await _repository.AddEntryAsync(entry, transaction);
                    transaction.Commit();
                    _logger.LogInformation("Created ledger entry: {EntryId}", entry.Id);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.LogError(ex, "Failed to create ledger entry");
                    throw;
                }
            }
        }

        /// <summary>
        /// Retrieves a collection of ledger entries associated with a specified region.
        /// </summary>
        /// <param name="region">The region for which to retrieve ledger entries.</param>
        /// <returns>A task representing the asynchronous operation, with a collection of ledger entries as the result.</returns>
        /// <exception cref="ArgumentException">Thrown if the region is null or empty.</exception>
        /// <exception cref="Exception">Thrown if there is an error retrieving the entries from the repository.</exception>
        public async Task<IEnumerable<LedgerEntry>> GetEntriesByRegionAsync(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
            {
                throw new ArgumentException("Region cannot be null or empty", nameof(region));
            }

            try
            {
                var entries = await _repository.GetEntriesByRegionAsync(region);
                return entries ?? Enumerable.Empty<LedgerEntry>();
            }
            catch (Exception ex)
            {
                // Log the error and rethrow or handle as appropriate for your application's context
                _logger.LogError(ex, "Failed to retrieve ledger entries for region: {Region}", region);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a collection of ledger entries associated with a specified account.
        /// </summary>
        /// <param name="account">The account for which to retrieve ledger entries.</param>
        /// <returns>A task representing the asynchronous operation, with a collection of ledger entries as the result.</returns>
        /// <exception cref="ArgumentException">Thrown if the account is null or empty.</exception>
        /// <exception cref="Exception">Thrown if there is an error retrieving the entries from the repository.</exception>
        public async Task<IEnumerable<LedgerEntry>> GetEntriesByAccountAsync(string account)
        {
            if (string.IsNullOrWhiteSpace(account))
            {
                throw new ArgumentException("Account cannot be null or empty", nameof(account));
            }

            try
            {
                var entries = await _repository.GetEntriesByAccountAsync(account);
                return entries ?? Enumerable.Empty<LedgerEntry>();
            }
            catch (Exception ex)
            {
                // Log the error and rethrow or handle as appropriate for your application's context
                _logger.LogError(ex, "Failed to retrieve ledger entries for account: {Account}", account);
                throw;
            }
        }
    }
}
