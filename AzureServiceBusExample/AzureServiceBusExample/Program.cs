using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceBus.Samples
{
    class Program
    {
        private static DataTable issues;
        private static List<BrokeredMessage> MessageList = new List<BrokeredMessage>();
        private static string ServiceNamespace;
        private static string sasKeyName = "RootManageSharedAccessKey";
        private static string sasKeyValue;

        static void Main(string[] args)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            NamespaceManager ConnectorNamespaceMgr = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!ConnectorNamespaceMgr.QueueExists("TestQueue"))
            {
                ConnectorNamespaceMgr.CreateQueue("TestQueue");
            }

            QueueClient Client = QueueClient.CreateFromConnectionString(connectionString, "TestQueue", ReceiveMode.PeekLock);

            for (int i = 0; i < 5; i++)
            {
                // Create message, passing a string message for the body.
                BrokeredMessage message = new BrokeredMessage("Test message " + i);

                // Set some addtional custom app-specific properties.
                message.Properties["TestProperty"] = "TestValue";
                message.Properties["Message number"] = i;

                // Send message to the queue.
                Client.Send(message);
            }
            ReceiveQueueMessages(Client);
        }

        static void ReceiveQueueMessages(QueueClient Client)
        {
            // Configure the callback options.
            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);

            // Callback to handle received messages.
            Client.OnMessage((message) =>
            {
                try
                {
                    // Process message from queue.
                    Console.WriteLine("Body: " + message.GetBody<string>());
                    Console.WriteLine("MessageID: " + message.MessageId);
                    Console.WriteLine("Test Property: " +
                    message.Properties["TestProperty"]);

                    // Remove message from queue.
                    message.Complete();
                }
                catch (Exception)
                {
                    // Indicates a problem, unlock message in queue.
                    message.Abandon();
                }
            }, options);
            Console.ReadLine();
        }

    }
}
