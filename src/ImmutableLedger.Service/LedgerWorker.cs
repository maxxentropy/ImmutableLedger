using Amazon.SQS.Model;
using Amazon.SQS;
using ImmutableLedger.Application.Interfaces;
using System.Text.Json;

namespace ImmutableLedger.Service
{
    public class LedgerWorker : BackgroundService
    {
        private readonly ILogger<LedgerWorker> _logger;
        private readonly IAmazonSQS _sqsClient;
        private readonly ILedgerService _ledgerService;
        private readonly string _queueUrl;
        private readonly SemaphoreSlim _semaphore;

        public LedgerWorker(
            ILogger<LedgerWorker> logger,
            IAmazonSQS sqsClient,
            ILedgerService ledgerService,
            IConfiguration configuration)
        {
            _logger = logger;
            _sqsClient = sqsClient;
            _ledgerService = ledgerService;
            _queueUrl = configuration["AWS:SQS:QueueUrl"];
            _semaphore = new SemaphoreSlim(1, 1); // Allow only one thread at a time
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LedgerWorker started at: {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var request = new ReceiveMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        MaxNumberOfMessages = 10,
                        WaitTimeSeconds = 20
                    };

                    var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                    foreach (var message in response.Messages)
                    {
                        try
                        {
                            await ProcessMessageAsync(message.Body);
                            await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                            _logger.LogInformation("Processed and deleted message: {MessageId}", message.MessageId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message: {MessageId}", message.MessageId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error receiving messages from SQS");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Wait before retrying
                }
            }

            _logger.LogInformation("LedgerWorker stopped at: {time}", DateTimeOffset.Now);
        }

        private async Task ProcessMessageAsync(string messageBody)
        {
            var ledgerEntry = JsonSerializer.Deserialize<LedgerEntryMessage>(messageBody);

            if (ledgerEntry == null)
            {
                throw new InvalidOperationException("Failed to deserialize message body to LedgerEntryMessage");
            }

            await _semaphore.WaitAsync();
            try
            {
                await _ledgerService.CreateEntryAsync(
                    ledgerEntry.AccountFrom,
                    ledgerEntry.AccountTo,
                    ledgerEntry.Amount,
                    ledgerEntry.Description,
                    ledgerEntry.Region
                );
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public class LedgerEntryMessage
    {
        public string AccountFrom { get; set; }
        public string AccountTo { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Region { get; set; }
    }
}
