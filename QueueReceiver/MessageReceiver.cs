using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QueueReceiver
{
    public static class MessageReceiver
    {
        static readonly string  stringConnectionSas = @"Endpoint=sb://idgordersrct.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=o3poGu91Yvglzl+ofhncCtolPMcyQFFUDwM145J2xQs=";
        static readonly string queueName = @"capturenotification";

        public static async Task ReceiveMessageAsync()
        {
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            //client = new ServiceBusClient(stringConnectionSas, clientOptions);
            // since ServiceBusClient implements IAsyncDisposable we create it with "await using"
            await using var client = new ServiceBusClient(stringConnectionSas, clientOptions);
             // create the sender
             ServiceBusSender sender = client.CreateSender(queueName);

            // create a message that we can send. UTF-8 encoding is used when providing a string.
            ServiceBusMessage message = new ServiceBusMessage("Hello world!");

            // send the message
            await sender.SendMessageAsync(message);

            // create a receiver that we can use to receive the message
            ServiceBusReceiver receiver = client.CreateReceiver(queueName);

            // the received message is a different type as it contains some service set properties
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

            // get the message body as a string
            string body = receivedMessage.Body.ToString();
            Console.WriteLine(body);

            //Console.ReadKey();
        }
        public static async Task ReceiveAllMessageAsync()
        {

            // the client that owns the connection and can be used to create senders and receivers
            ServiceBusClient client;

            // the processor that reads and processes messages from the queue
            ServiceBusProcessor processor;
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // Set the transport type to AmqpWebSockets so that the ServiceBusClient uses port 443. 
            // If you use the default AmqpTcp, make sure that ports 5671 and 5672 are open.

            

            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            client = new ServiceBusClient(stringConnectionSas, clientOptions);

            // create a processor that we can use to process the messages
            // TODO: Replace the <QUEUE-NAME> placeholder
            processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            try
            {
                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        // handle received messages
        private static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received: {body}");

            // complete the message. message is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
    }
}
