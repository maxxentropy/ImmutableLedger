using ImmutableLedger.Domain.Entities;
using ImmutableLedger.Domain.Interfaces;
using ImmutableLedger.Infrastructure.Sharding;
using Npgsql;
using System.Data;

namespace ImmutableLedger.Infrastructure.Data
{
    /// <summary>
    /// Implements the ILedgerRepository interface using PostgreSQL with sharding support.
    /// This class is responsible for all data access operations related to ledger entries.
    /// </summary>
    public class ShardedPostgreSqlLedgerRepository : ILedgerRepository
    {
        private readonly ShardManager _shardManager;

        /// <summary>
        /// Initializes a new instance of the ShardedPostgreSqlLedgerRepository class.
        /// </summary>
        /// <param name="shardManager">The shard manager to use for determining the appropriate shard.</param>
        public ShardedPostgreSqlLedgerRepository(ShardManager shardManager)
        {
            _shardManager = shardManager;
        }

        /// <summary>
        /// Begins a new transaction for write operations.
        /// </summary>
        /// <returns></returns>
        public async Task<IDbTransaction> BeginTransactionAsync()
        {
            var connection = GetWriteConnection("default", "default"); // You might want to adjust this based on your sharding strategy
            await connection.OpenAsync();
            return await connection.BeginTransactionAsync();
        }

        /// <summary>
        /// Gets a database connection for write operations based on the entry's region and account.
        /// </summary>
        private NpgsqlConnection GetWriteConnection(string region, string account)
        {
            var config = _shardManager.GetDatabaseConfig(region, account);
            return new NpgsqlConnection(config.ConnectionString);
        }

        private NpgsqlConnection GetReadConnection(string region, string account)
        {
            var config = _shardManager.GetDatabaseConfig(region, account);
            string connectionString = _shardManager.GetReadReplicaConnectionString(config);
            return new NpgsqlConnection(connectionString);
        }

        public async Task AddEntryAsync(LedgerEntry entry, IDbTransaction transaction = null)
        {
            NpgsqlConnection connection;
            bool ownConnection = false;

            if (transaction == null)
            {
                // Open a new connection for the write operation
                connection = GetWriteConnection(entry.Region, entry.AccountFrom);
                await connection.OpenAsync();
                ownConnection = true;
            }
            else
            {
                // Use the existing connection from the transaction
                connection = (NpgsqlConnection)transaction.Connection;
            }

            try
            {
                const string sql = @"
                    INSERT INTO ledger_entries (id, timestamp, account_from, account_to, amount, description, region)
                    VALUES (@Id, @Timestamp, @AccountFrom, @AccountTo, @Amount, @Description, @Region)";

                using var command = new NpgsqlCommand(sql, connection);
                if (transaction != null)
                    command.Transaction = (NpgsqlTransaction)transaction;

                command.Parameters.AddWithValue("@Id", entry.Id);
                command.Parameters.AddWithValue("@Timestamp", entry.Timestamp);
                command.Parameters.AddWithValue("@AccountFrom", entry.AccountFrom);
                command.Parameters.AddWithValue("@AccountTo", entry.AccountTo);
                command.Parameters.AddWithValue("@Amount", entry.Amount);
                command.Parameters.AddWithValue("@Description", entry.Description);
                command.Parameters.AddWithValue("@Region", entry.Region);

                await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if (ownConnection)
                    await connection.CloseAsync();
            }
        }

        public async Task<IEnumerable<LedgerEntry>> GetEntriesByRegionAsync(string region)
        {
            var entries = new List<LedgerEntry>();
            foreach (var shardConfig in _shardManager.GetAllShards())
            {
                using var connection = new NpgsqlConnection(_shardManager.GetReadReplicaConnectionString(shardConfig));
                await connection.OpenAsync();

                var sql = @"
                    SELECT id, timestamp, account_from, account_to, amount, description, region
                    FROM ledger_entries
                    WHERE region = @Region";

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Region", region);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    entries.Add(new LedgerEntry(
                        reader.GetString(2), // account_from
                        reader.GetString(3), // account_to
                        reader.GetDecimal(4), // amount
                        reader.GetString(5), // description
                        reader.GetString(6)  // region
                    ));
                }
            }

            return entries;
        }

        public async Task<IEnumerable<LedgerEntry>> GetEntriesByAccountAsync(string account)
        {
            var entries = new List<LedgerEntry>();
            foreach (var shardConfig in _shardManager.GetAllShards())
            {
                using var connection = new NpgsqlConnection(_shardManager.GetReadReplicaConnectionString(shardConfig));
                await connection.OpenAsync();

                var sql = @"
                    SELECT id, timestamp, account_from, account_to, amount, description, region
                    FROM ledger_entries
                    WHERE account_from = @Account OR account_to = @Account";

                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Account", account);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    entries.Add(new LedgerEntry(
                        reader.GetString(2), // account_from
                        reader.GetString(3), // account_to
                        reader.GetDecimal(4), // amount
                        reader.GetString(5), // description
                        reader.GetString(6)  // region
                    ));
                }
            }

            return entries;
        }
    }
}
