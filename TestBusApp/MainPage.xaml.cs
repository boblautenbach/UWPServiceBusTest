using Amqp;
using Amqp.Framing;
using Microsoft.WindowsAzure.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestBusApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

       // Endpoint=sb://pitalker.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=5/tR7cHnRKvW6ZPIM1EqKCYw856mE8tU/qcy1wQD2/k=
        string connectionString = "Endpoint=sb://pitalker.servicebus.windows.net/;SharedSecretIssuer=RootManageSharedAccessKey;SharedSecretValue=5/tR7cHnRKvW6ZPIM1EqKCYw856mE8tU/qcy1wQD2/k=";
        static Subscription sub;

        Amqp.Connection amqpConnection;
        string amqpEventhubHostFormat = "amqps://{0}:{1}@{2}.servicebus.windows.net";
        Address ampsAddress;
        Session amqpSession;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            // Create the topic if it does not exist already.
            SenderLink senderObj = null;
            try
            {
                // Amqp Code
               // amqpSession = new Session(amqpConnection);
                var senderSubscriptionId = "bobs";
                var topic = "chats";
              
                senderObj = new SenderLink(amqpSession, senderSubscriptionId, topic);
             
                for (var i = 0; i < 10; i++)
                {
                    // Create message
                    var message = new Amqp.Message($"Received message {i}");

                    // Add a meesage id
                    message.Properties = new Properties() { MessageId = Guid.NewGuid().ToString() };

                    // Add some message properties
                    message.ApplicationProperties = new ApplicationProperties();
                    message.ApplicationProperties["MyProperty"] = typeof(string).FullName;

                    // Send message
                    await senderObj.SendAsync(message);
                }


                //Azure.Messaging.Managed
                //var top = new Topic("chats", connectionString1);
                //// await top.SendAsync(new Microsoft.WindowsAzure.Messaging.Message("Hello World"));
                //await top.SendAsync<string>("Hello There");

            }
            catch (Exception ex)
            {
                SendToView("Error " + ex.Message);
            }
           await  senderObj.CloseAsync();
        }


        private async Task SendToView(string message)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                txtLog.Text = message;
            });
        }

        void OnMessageCallback(ReceiverLink receiver, Amqp.Message message)
        {
            try

            {
                if (message.ApplicationProperties != null)
                {
                    // You can read the custom property
                    var messageType = message.ApplicationProperties["MyProperty"];

                    // Variable to save the body of the message.
                    string body = string.Empty;

                    // Get the body
                    var rawBody = message.Body;

                    // If the body is byte[] assume it was sent as a BrokeredMessage  
                    // and deserialize it using a XmlDictionaryReader
                    if (rawBody is byte[])
                    {
                        using (var reader = XmlDictionaryReader.CreateBinaryReader(
                            new MemoryStream(rawBody as byte[]),
                            null,
                            XmlDictionaryReaderQuotas.Max))
                        {
                            var doc = new XmlDocument();
                            doc.LoadXml(reader.ToString());
                            body = doc.InnerText;
                        }
                    }
                    else // Asume the body is a string
                    {
                        body = rawBody.ToString();
                    }

                    // Write the body to the Console.
                    SendToView(rawBody.ToString());

                    // Accept the messsage.
                    receiver.Accept(message);
                 
                }
            }
            catch (Exception ex)
            {
                receiver.Reject(message);
                SendToView(ex.Message);
            }

        }

        private async void OnConnect(object sender, RoutedEventArgs e)
        {
            try
            {
                string fg = "amqps://{0}:{1}@{2}.servicebus.windows.net";
                ampsAddress = new Address(string.Format(fg, "RootManageSharedAccessKey", Uri.EscapeDataString("5/tR7cHnRKvW6ZPIM1EqKCYw856mE8tU/qcy1wQD2/k="), "pitalker"));

                amqpConnection =  await  Amqp.Connection.Factory.CreateAsync(ampsAddress);
                
                var receiverSubscriptionId = "me.amqp.recieiver";

                // Name of the topic you will be sending messages
                var topic = "chats";

                // Name of the subscription you will receive messages from
                var subscription = "bob";

                amqpSession = new Session(amqpConnection);
                var consumer = new ReceiverLink(amqpSession, receiverSubscriptionId, $"{topic}/Subscriptions/{subscription}");
                
                // Start listening
                consumer.Start(5, OnMessageCallback);


                //sub = new Subscription("chats", "bob", connectionString1);

                //sub.OnMessage<string>((message) =>
                //{
                //    try
                //    {
                //        SendToView("Got to OnMessage Call");
                //    }
                //    catch (Exception r)
                //    {
                //        SendToView("Error in OnMessage Call " + r.Message);
                //    }
                //});

            }
            catch (Exception ex)
            {
                SendToView("Error " + ex.Message);
            }

        }
         void MessageQueHandler(Microsoft.WindowsAzure.Messaging.Message messsage)
        {
            try
            {
                SendToView("Got to OnMessage Call");
            }
            catch (Exception r)
            {
                SendToView("Error in OnMessage Call " + r.Message);
            }
        }
    }
}
