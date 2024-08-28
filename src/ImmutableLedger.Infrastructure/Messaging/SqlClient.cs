using Amazon.SQS;
using Amazon.SQS.Model;
using ImmutableLedger.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImmutableLedger.Infrastructure.Messaging
{
    public class SqsClient : BackgroundService
    {
        private readonly AmazonSQSClient _sqsClient;
        private readonly IMessageHandler _messageHandler;
        private readonly ILogger<SqsClient> _logger;
        private readonly string _queueUrl;

        public SqsClient(AmazonSQSClient sqsClient, IMessageHandler messageHandler, ILogger<SqsClient> logger, IConfiguration configuration)
        {
            _sqsClient = sqsClient;
            _messageHandler = messageHandler;
            _logger = logger;
            _queueUrl = configuration["AWS:SQS:QueueUrl"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
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
                        await _messageHandler.HandleMessageAsync(message.Body);
                        await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message: {MessageId}", message.MessageId);
                    }
                }
            }
        }
    }
}
