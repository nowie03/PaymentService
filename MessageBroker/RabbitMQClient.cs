﻿using Newtonsoft.Json;
using PaymentService.Models;
using RabbitMQ.Client;
using System.Text;

namespace PaymentService.MessageBroker
{
    public class RabbitMQClient : IMessageBrokerClient, IDisposable
    {
        private ConnectionFactory _connectionFactory;
        private RabbitMQ.Client.IConnection _connection;
        private RabbitMQ.Client.IModel _channel;
        private string _queueName = "service-queue";

        private MessageHandler<Order> _messageHandler;

        //create Dbcontext 

        public RabbitMQClient(IServiceProvider serviceProvider)
        {


            SetupClient(serviceProvider);

        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        private void SetupClient(IServiceProvider serviceProvider)
        {
            //Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
            _connectionFactory = new ConnectionFactory
            {
                HostName = "message-queue"
            };
            try
            {
                //Create the RabbitMQ connection using connection factory details as i mentioned above
                _connection = _connectionFactory.CreateConnection();
                //Here we create channel with session and model
                _channel = _connection.CreateModel();
                //declare the queue after mentioning name and a few property related to that


                _messageHandler = new(_channel, serviceProvider);
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public void SendMessage(Models.Message eventMessage)
        {

            //Serialize the message

            if (_channel == null)
                return;




            string json = JsonConvert.SerializeObject(eventMessage);


            var body = Encoding.UTF8.GetBytes(json);


            //put the data on to the product queue
            _channel.BasicPublish(exchange: "", routingKey: _queueName, body: body);
        }

        public void ReceiveMessage()
        {
            if (_channel == null) return;

            

            var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(_channel);
            consumer.Received += _messageHandler.HandleMessage;
            //read the message
            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

        }

        public ulong GetNextSequenceNumber() => (ulong)(_channel?.NextPublishSeqNo);
    }
}
