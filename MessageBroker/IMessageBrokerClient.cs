using PaymentService.Models;

namespace PaymentService.MessageBroker
{
    public interface IMessageBrokerClient
    {
        public void SendMessage(Message eventMessage);

        public void ReceiveMessage();
    }
}
