using Newtonsoft.Json;
using PaymentService.Constants;
using PaymentService.Context;
using PaymentService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace PaymentService.MessageBroker
{
    public class MessageHandler<T> where T : Order
    {
        private readonly IModel _channel;

        private readonly IServiceProvider _serviceProvider;

        public MessageHandler(IModel channel, IServiceProvider serviceProvider)
        {
            //get servicecontext from injected service container
            _serviceProvider = serviceProvider;

            _channel = channel;

            Console.WriteLine("message handler created");
        }

        public async void HandleMessage(object model, BasicDeliverEventArgs eventArgs)
        {
            using var scope = _serviceProvider.CreateScope();
            var _serviceContext = scope.ServiceProvider.GetRequiredService<ServiceContext>();

            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            Console.WriteLine($"message received from queue {message}");

            Message<Order>? eventMessage = JsonConvert.DeserializeObject<Message<Order>>(message);
           

            // Perform the message handling logic here based on the event message
            if (eventMessage!=null &&  eventMessage.EventType == EventTypes.PAYMENT_INITIATED)
            {
                // Handle the PAYMENT_INITIATED event
                Order order = eventMessage.Payload;
                Console.WriteLine(order);

                try
                {
                    Payment payment = new()
                    {
                        OrderId = order.Id,
                        Status= Enums.PaymentStatus.PENDING,
                        CreatedAt = DateTime.Now
                    };

                    await _serviceContext.Payments.AddAsync(payment);
                    await _serviceContext.SaveChangesAsync();

                _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error when creating payment for order id {order.Id} {ex.Message}");
                }

                //acknowldege queue of successful consume 
            }


            

        }
    }
}
