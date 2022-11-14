using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace QueueReceiver
{
    class Program
    {
        public static void Main(string[] args)
        {
            MessageReceiver.ReceiveMessageAsync().GetAwaiter().GetResult();
            //MessageReceiver.ReceiveAllMessageAsync().GetAwaiter().GetResult();
        }
        
    }
}
