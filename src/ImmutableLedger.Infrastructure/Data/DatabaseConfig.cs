namespace ImmutableLedger.Infrastructure.Data
{
    public class DatabaseConfig
    {
        public string ConnectionString { get; set; }
        public string ShardKey { get; set; }
        public List<string> ReadReplicaConnectionStrings { get; set; } = new List<string>();
    }
}
