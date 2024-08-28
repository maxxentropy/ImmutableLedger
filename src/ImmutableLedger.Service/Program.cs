using Amazon.Runtime;
using Amazon;
using Amazon.SQS;
using ImmutableLedger.Application.Interfaces;
using ImmutableLedger.Application.Services;
using ImmutableLedger.Domain.Interfaces;
using ImmutableLedger.Infrastructure.Data;
using ImmutableLedger.Infrastructure.Messaging;
using ImmutableLedger.Infrastructure.Sharding;

namespace ImmutableLedger.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<LedgerWorker>();
                    services.AddHostedService<SqsClient>();

                    // Configure AWS SQS
                    services.AddSingleton(sp =>
                    {
                        var config = sp.GetRequiredService<IConfiguration>();
                        var region = config["AWS:Region"];
                        var credentials = new EnvironmentVariablesAWSCredentials();
                        return new AmazonSQSClient(credentials, RegionEndpoint.GetBySystemName(region));
                    });

                    // Configure DI
                    ConfigureServices(services);
                });

        private static void ConfigureServices(IServiceCollection services)
        {
            var shardMap = new Dictionary<string, DatabaseConfig>
            {
                ["SHARD-00"] = new DatabaseConfig
                {
                    ConnectionString = "Host=shard1.example.com;Database=ledger;Username=user;Password=password;",
                    ShardKey = "SHARD-00",
                    ReadReplicaConnectionStrings = new List<string>
                    {
                        "Host=shard1-read1.example.com;Database=ledger;Username=user;Password=password;",
                        "Host=shard1-read2.example.com;Database=ledger;Username=user;Password=password;"
                    }
                },
                ["SHARD-01"] = new DatabaseConfig
                {
                    ConnectionString = "Host=shard2.example.com;Database=ledger;Username=user;Password=password;",
                    ShardKey = "SHARD-01",
                    ReadReplicaConnectionStrings = new List<string>
                    {
                        "Host=shard2-read1.example.com;Database=ledger;Username=user;Password=password;",
                        "Host=shard2-read2.example.com;Database=ledger;Username=user;Password=password;"
                    }
                },
            };

            var shardManager = new ShardManager(shardMap);
            services.AddSingleton<ILedgerRepository>(new ShardedPostgreSqlLedgerRepository(shardManager));
            services.AddScoped<ILedgerService, LedgerService>();
            services.AddScoped<IMessageHandler, MessageHandler>();
        }
    }
}