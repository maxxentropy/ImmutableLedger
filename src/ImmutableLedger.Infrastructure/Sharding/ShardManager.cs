using ImmutableLedger.Infrastructure.Data;

namespace ImmutableLedger.Infrastructure.Sharding
{
    using System;
    using System.Collections.Generic;

    public class ShardManager
    {
        // Dictionary that maps shard keys to their corresponding database configurations
        private readonly Dictionary<string, DatabaseConfig> _shardMap;

        // Instance of the AdaptiveShardingStrategy to determine which shard key to use
        private readonly AdaptiveShardingStrategy _shardingStrategy;

        // Random number generator used to select a read replica connection string
        private readonly Random _random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShardManager"/> class.
        /// </summary>
        /// <param name="shardMap">A dictionary mapping shard keys to their respective database configurations.</param>
        public ShardManager(Dictionary<string, DatabaseConfig> shardMap)
        {
            _shardMap = shardMap;
            _shardingStrategy = new AdaptiveShardingStrategy(_shardMap.Count);
        }

        /// <summary>
        /// Retrieves the database configuration for a given region and account based on the sharding strategy.
        /// </summary>
        /// <param name="region">The region associated with the request.</param>
        /// <param name="account">The account identifier associated with the request.</param>
        /// <returns>The database configuration associated with the calculated shard key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no database configuration is found for the calculated shard key.</exception>
        public DatabaseConfig GetDatabaseConfig(string region, string account)
        {
            // Determine the shard key using the adaptive sharding strategy
            string shardKey = _shardingStrategy.GetShardKey(region, account);

            // Attempt to retrieve the database configuration for the shard key
            if (_shardMap.TryGetValue(shardKey, out var config))
            {
                return config;
            }

            // If no configuration is found for the shard key, throw an exception
            throw new KeyNotFoundException($"No database configuration found for shard key: {shardKey}");
        }

        /// <summary>
        /// Retrieves a connection string for a read replica, if available, otherwise returns the primary connection string.
        /// </summary>
        /// <param name="config">The database configuration for which to retrieve the connection string.</param>
        /// <returns>A connection string for a read replica or the primary database connection string if no read replicas are available.</returns>
        public string GetReadReplicaConnectionString(DatabaseConfig config)
        {
            // Check if there are any read replicas available
            if (config.ReadReplicaConnectionStrings.Count > 0)
            {
                // Randomly select one of the read replica connection strings
                return config.ReadReplicaConnectionStrings[_random.Next(config.ReadReplicaConnectionStrings.Count)];
            }

            // If no read replicas are available, fall back to the primary connection string
            return config.ConnectionString;
        }

        /// <summary>
        /// Retrieves the database configurations for all shards.
        /// </summary>
        /// <returns>An enumerable collection of all database configurations.</returns>
        public IEnumerable<DatabaseConfig> GetAllShards()
        {
            // Return all database configurations from the shard map
            return _shardMap.Values;
        }
    }

}
