using Amqp;
using Amqp.Framing;
using Microsoft.WindowsAzure.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

        string connectionString = "Endpoint=sb://pitalk.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YYj8bgHbV17s0qYfRX/3iDjWo1B3x2jkip0Req/omWw=";
        Subscription sub = null;

        string amqpEventhubHostFormat = "amqps://{0}:{1}@{2}.servicebus.windows.net";
        Address ampsAddress;
        Amqp.Connection amqpConnection = null;

        public MainPage()
        {
            this.InitializeComponent();


            //ampsAddress = new Address(string.Format(amqpEventhubHostFormat, "RootManageSharedAccessKey", Uri.EscapeDataString("YYj8bgHbV17s0qYfRX/3iDjWo1B3x2jkip0Req/omWw="), "pitalk"));
            //amqpConnection = new Amqp.Connection(ampsAddress);
        }

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            // Create the topic if it does not exist already.
            try
            {
                //Amqp Code
                //var amqpSession = new Session(amqpConnection);
                //var senderSubscriptionId = "me.amqp.sender";
                //var topic = "messages";
                //var sender = new SenderLink(amqpSession, senderSubscriptionId, topic);
                //var message = new Amqp.Message();
                //message.Properties = new Properties();
                //message.Properties.Subject = "mymessagetype";
                //message.ApplicationProperties = new ApplicationProperties();
                //message.ApplicationProperties["MyProperty"] = "Hello World!";
                //sender.Send(message);



                //Azure.Messaging.Managed
                var topic = new Topic("messages", connectionString);
                await topic.SendAsync(new Microsoft.WindowsAzure.Messaging.Message("Hello World"));
            }
            catch (Exception ex)
            {
                SendToView("Error " + ex.Message);
            }
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
                var value = message.ApplicationProperties["MyProperty"];
                // Do something with the value
                receiver.Accept(message);
                receiver.SetCredit(5);
                SendToView(value.ToString());
            }
            catch (Exception ex)
            {
                SendToView(ex.Message);
            }
        }

        private void OnConnect(object sender, RoutedEventArgs e)
        {
            try
            {
                ////Amqp Code
                //var receiverSubscriptionId = "me.amqp.recieiver";

                //// Name of the topic you will be sending messages
                //var topic = "messages";

                //// Name of the subscription you will receive messages from
                //var subscription = "bob";

                //var amqpSession = new Session(amqpConnection);
                //var consumer = new ReceiverLink(amqpSession, receiverSubscriptionId, $"{topic}/Subscriptions/{subscription}");

                //// Start listening
                //consumer.Start(5, OnMessageCallback);


                sub = new Subscription("messages", "bob", connectionString);

                sub.OnMessage<Microsoft.WindowsAzure.Messaging.Message>((message) =>
                {
                    try
                    {
                        SendToView("Got to OnMessage Call");
                    }
                    catch (Exception r)
                    {
                        SendToView("Error in OnMessage Call " + r.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                SendToView("Error " + ex.Message);
            }

        }
    }
}
