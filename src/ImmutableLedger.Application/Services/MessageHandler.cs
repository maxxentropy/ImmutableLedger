using ImmutableLedger.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ImmutableLedger.Application.Services
{
    /// 
    /// Implements the IMessageHandler interface, providing logic for processing incoming messages.
    /// This handler deserializes messages and uses the LedgerService to create new ledger entries.
    /// 
    public class MessageHandler : IMessageHandler
    {
        private readonly ILedgerService _ledgerService;
        private readonly ILogger<MessageHandler> _logger;

        public MessageHandler(ILedgerService ledgerService, ILogger<MessageHandler> logger)
        {
            _ledgerService = ledgerService;
            _logger = logger;
        }

        /// <summary>
        /// Handles an incoming message by deserializing it into a LedgerEntryMessage object
        /// and processing the entry by calling the ledger service.
        /// </summary>
        /// <param name="message">The JSON message containing the ledger entry details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the message is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if deserialization fails or results in a null entry.</exception>
        /// <exception cref="Exception">Thrown if there is an error creating the ledger entry.</exception>
        public async Task HandleMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            }

            LedgerEntryMessage? entry;
            try
            {
                entry = JsonSerializer.Deserialize<LedgerEntryMessage>(message);
            }
            catch (JsonException ex)
            {
                // Log and handle the JSON deserialization exception
                _logger.LogError(ex, "Failed to deserialize the message: {Message}", message);
                throw new InvalidOperationException("Failed to deserialize the message.", ex);
            }

            if (entry == null)
            {
                throw new InvalidOperationException("Deserialized message resulted in a null entry.");
            }

            try
            {
                await _ledgerService.CreateEntryAsync(
                    entry.AccountFrom,
                    entry.AccountTo,
                    entry.Amount,
                    entry.Description,
                    entry.Region
                );
            }
            catch (Exception ex)
            {
                // Log and handle the exception from the ledger service
                _logger.LogError(ex, "Failed to create ledger entry for message: {Message}", message);
                throw;
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
