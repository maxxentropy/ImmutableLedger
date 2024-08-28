namespace ImmutableLedger.Application.Interfaces
{
    /// 
    /// Defines the contract for handling incoming messages.
    /// This interface abstracts the message processing logic,
    /// allowing for different message formats and processing strategies.
    /// 
    public interface IMessageHandler
    {
        /// 
        /// Handles an incoming message asynchronously.
        /// 
        /// The message to be processed.
        /// A task representing the asynchronous operation.
        Task HandleMessageAsync(string message);
    }
}
