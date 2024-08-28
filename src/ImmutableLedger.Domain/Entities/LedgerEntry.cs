namespace ImmutableLedger.Domain.Entities
{
    public class LedgerEntry
    {
        /// 
        /// Unique identifier for the ledger entry.
        /// 
        public Guid Id { get; }

        /// 
        /// The timestamp when the entry was created.
        /// 
        public DateTime Timestamp { get; }

        /// 
        /// The account from which the transaction originates.
        /// 
        public string AccountFrom { get; }

        /// 
        /// The account to which the transaction is directed.
        /// 
        public string AccountTo { get; }

        /// 
        /// The amount of the transaction.
        /// 
        public decimal Amount { get; }

        /// 
        /// A description of the transaction.
        /// 
        public string Description { get; }

        /// 
        /// The region associated with this ledger entry.
        /// This is used for sharding purposes.
        /// 
        public string Region { get; }

        /// 
        /// Creates a new ledger entry.
        /// 
        /// The source account.
        /// The destination account.
        /// The transaction amount.
        /// A description of the transaction.
        /// The region associated with this entry.        
        public LedgerEntry(string accountFrom, string accountTo, decimal amount, string description, string region)
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            AccountFrom = accountFrom;
            AccountTo = accountTo;
            Amount = amount;
            Description = description;
            Region = region;
        }
    }
}
