namespace ImmutableLedger.Infrastructure.Sharding
{
    public class AdaptiveShardingStrategy
    {
        // Dictionary to track the load of each region
        private readonly Dictionary<string, int> _regionLoad;

        // Dictionary to track the load of each account's first character
        private readonly Dictionary<char, int> _accountLoad;

        // Total number of shards available
        private readonly int _shardCount;

        // Lock object to ensure thread safety when updating loads and calculating shard keys
        private readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveShardingStrategy"/> class.
        /// </summary>
        /// <param name="shardCount">The total number of shards to distribute the load across.</param>
        public AdaptiveShardingStrategy(int shardCount)
        {
            _regionLoad = new Dictionary<string, int>();
            _accountLoad = new Dictionary<char, int>();
            _shardCount = shardCount;
        }

        /// <summary>
        /// Determines the shard key for a given region and account by calculating a combined score
        /// based on the load of the region and the first character of the account.
        /// </summary>
        /// <param name="region">The region associated with the entry.</param>
        /// <param name="account">The account identifier (a string) associated with the entry.</param>
        /// <returns>A shard key in the format "SHARD-XX" where XX is the shard index.</returns>
        public string GetShardKey(string region, string account)
        {
            lock (_lockObject)
            {
                // Update the load statistics for the region and the first character of the account
                UpdateLoad(region, account[0]);

                // Retrieve the load scores for the region and the account's first character
                int regionScore = _regionLoad.GetValueOrDefault(region, 0);
                int accountScore = _accountLoad.GetValueOrDefault(account[0], 0);

                // Combine the scores to determine the overall load influence
                int combinedScore = regionScore + accountScore;

                // Calculate the shard index by taking the modulus of the combined score with the shard count
                int shardIndex = combinedScore % _shardCount;

                // Return the shard key formatted as "SHARD-XX"
                return $"SHARD-{shardIndex:D2}";
            }
        }

        /// <summary>
        /// Updates the load tracking dictionaries for the specified region and account's first character.
        /// Also performs simple load balancing by resetting the scores if they exceed a certain threshold.
        /// </summary>
        /// <param name="region">The region associated with the entry.</param>
        /// <param name="accountFirstChar">The first character of the account identifier.</param>
        private void UpdateLoad(string region, char accountFirstChar)
        {
            // Increment the load count for the region; if the region is not tracked yet, initialize it
            if (!_regionLoad.ContainsKey(region))
                _regionLoad[region] = 0;
            _regionLoad[region]++;

            // Increment the load count for the account's first character; if not tracked, initialize it
            if (!_accountLoad.ContainsKey(accountFirstChar))
                _accountLoad[accountFirstChar] = 0;
            _accountLoad[accountFirstChar]++;

            // Perform load balancing by resetting the scores if they exceed a threshold (1000 in this case)
            if (_regionLoad.Values.Max() > 1000 || _accountLoad.Values.Max() > 1000)
            {
                // Reduce all region load scores by half
                foreach (var key in _regionLoad.Keys.ToList())
                    _regionLoad[key] /= 2;

                // Reduce all account load scores by half
                foreach (var key in _accountLoad.Keys.ToList())
                    _accountLoad[key] /= 2;
            }
        }
    }


}
