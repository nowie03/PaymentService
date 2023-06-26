﻿using PaymentService.MessageBroker;

namespace PaymentService.BackgroundServices
{
    public class MessageProcessingService:BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBrokerClient _messageBrokerClient;


        public MessageProcessingService(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            _messageBrokerClient = serviceProvider.GetRequiredService<IMessageBrokerClient>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                // Perform any additional background processing if needed
                try
                {
                    _messageBrokerClient.ReceiveMessage();
                }catch (Exception ex)
                {
                    Console.WriteLine("Error occured when receiving message");
                }
                await Task.Delay(1000, stoppingToken); // Delay between iterations to avoid high CPU usage
            }


        }
    }
}
