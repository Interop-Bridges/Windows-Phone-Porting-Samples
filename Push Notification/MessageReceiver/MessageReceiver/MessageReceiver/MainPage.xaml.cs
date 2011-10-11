// --------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Microsoft Corp">
// Copyright 2010 Microsoft Corp
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// ---------------------------------------------------------------------
namespace MessageReceiver
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Shapes;
    using System.Xml.Linq;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Notification;

    /// <summary>
    /// SubscriptionInfo is used for managing the data returned by the Azure service.
    /// </summary>
    public struct DeviceSubscriptionInfo
    {
        /// <summary>
        /// Gets or sets the name of the device subscription.
        /// </summary>
        public string Name { get; set; }
        //public string Name;

        /// <summary>
        /// Gets or sets the Description of the device subscription.
        /// </summary>
        public string Description { get; set; }
        //public string Description;
        /// <summary>
        /// Gets or sets a value indicating if isSubscribed is true.
        /// </summary>
        public bool IsSubscribed { get; set; }
        //public bool IsSubscribed;
    }

    /// <summary>
    /// Subscription is used to manage the data returned by the Azure service.
    /// </summary>
    internal struct Subscription
    {
        /// <summary>
        /// Gets or sets the name of the subscription.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the subscription.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// MainPage class for the WP7 application.
    /// </summary>
    public partial class MainPage : PhoneApplicationPage, IDisposable
    {
        private const string ChannelName = "Notifications.Phone.NotificationChannel";

        private const string BaseAddress = "http://127.0.0.1/push.svc";
        private const string BaseAddressDev = "http://127.0.0.1/WP7Device.svc";

        //After deploying to address, change the BaseAddress to the address of the deployed AzurePushMessage service
        //private const string BaseAddress = "http://wp7azuretest1.cloudapp.net/push.svc";
        //private const string BaseAddressDev = "http://wp7azuretest1.cloudapp.net/WP7Device.svc";

        private const string Username1 = "tony";
        private const string Password1 = "clifton";

        private Guid wp7DeviceId;
        private HttpNotificationChannel notificationChannel;
        private RestConnection pushRestConn;
        private RestConnection deviceRestConn;
        private string encodedUriStr;
        private string deviceIdStr;

        /// <summary>
        /// Initializes a new instance of the MainPage class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            // Initialize our channel to the service.
            this.pushRestConn = new RestConnection(BaseAddress, Username1, Password1);
            this.deviceRestConn = new RestConnection(BaseAddressDev, Username1, Password1);

            // If we have previously created the unique id, use it otherwise create one.
            if (IsolatedStorageSettings.ApplicationSettings.Contains("DeviceId"))
            {
                // Retrieve the unique id saved in the isolated storage.
                this.wp7DeviceId = (Guid)IsolatedStorageSettings.ApplicationSettings["DeviceId"];
            }
            else
            {
                // Create a new guid and save it in the isolated storage
                this.wp7DeviceId = Guid.NewGuid();
                IsolatedStorageSettings.ApplicationSettings["DeviceId"] = this.wp7DeviceId;
            }

            this.deviceIdStr = this.wp7DeviceId.ToString();
            this.SetupNotificationChannel();
        }

        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes
                = System.Text.UTF8Encoding.UTF8.GetBytes(toEncode);
            string returnValue
                  = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        /// <summary>
        /// Dispose() calls Dispose(true)
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The bulk of the clean-up code is implemented in Dispose(bool)
        /// </summary>
        /// <param name="disposing">Whether this is disposing or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free managed resources.
                if (this.notificationChannel != null)
                {
                    this.notificationChannel.Dispose();
                    this.notificationChannel = null;
                }
            }
        }

        private static void ParseXML(string xmlString)
        {
            XDocument xdoc = XDocument.Parse(xmlString);
        }

        private void SetupSubscriptionOptions()
        {
            // Set up notification channel and subscribe to notifications.
            this.pushRestConn.SendGetRequest("/subs/" + this.deviceIdStr, "subs");
            this.pushRestConn.DownloadHandler += new EventHandler<DownloadStringCompletedEventArgs>(this.PushRestConnDownloadHandler);
        }

        private void PushRestConnDownloadHandler(object sender, DownloadStringCompletedEventArgs e)
        {
            this.pushRestConn.DownloadHandler -= new EventHandler<DownloadStringCompletedEventArgs>(this.PushRestConnDownloadHandler);
            string userToken = (e as DownloadStringCompletedEventArgs).UserState as string;
            if (userToken == "subs")
            {
                XDocument xdoc = XDocument.Parse(e.Result);
                IEnumerable<XElement> xelements = xdoc.Descendants();

                var devSubscriptions = from element in 
                 xdoc.Descendants("{http://schemas.datacontract.org/2004/07/MsgHelperLib.Common}DeviceSubscriptionInfo")
                                    select new DeviceSubscriptionInfo
                                     {
                                         Name = element.Element("{http://schemas.datacontract.org/2004/07/MsgHelperLib.Common}Name").Value,
                                         Description = element.Element("{http://schemas.datacontract.org/2004/07/MsgHelperLib.Common}Description").Value,
                                         IsSubscribed = element.Element("{http://schemas.datacontract.org/2004/07/MsgHelperLib.Common}IsSubscribed").Value == "true" ? true : false
                                     };
                subscriptionsList.ItemsSource = devSubscriptions;
            }
        }

        private void CbClick(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if ((bool)cb.IsChecked)
            {
                this.SubscribeToNotification((string)cb.Tag);
            }
            else
            {
                this.UnSubscribeToNotification((string)cb.Tag);
            }
        }

        private void SubscribeToNotification(string p)
        {
            string deviceSubUrl = "/sub/add/" + p + "/" + this.deviceIdStr;
            this.pushRestConn.SendPostRequest(deviceSubUrl, string.Empty, "subscribe");
            this.pushRestConn.UploadHandler -= new EventHandler<UploadStringCompletedEventArgs>(this.PushRestConnUploadHandler);
        }

        private void UnSubscribeToNotification(string p)
        {
            string deviceSubUrl = "/sub/delete/" + p + "/" + this.deviceIdStr;
            this.pushRestConn.SendPostRequest(deviceSubUrl, string.Empty, "unsubscribe");
            this.pushRestConn.UploadHandler -= new EventHandler<UploadStringCompletedEventArgs>(this.PushRestConnUploadHandler);
        }

        private void SetupNotificationChannel()
        {
            // Find the channel with the given name
            this.notificationChannel = HttpNotificationChannel.Find(ChannelName);
            if (this.notificationChannel == null)
            {
                // channel did not exist. Create one
                this.notificationChannel = new HttpNotificationChannel(ChannelName);

                // hookup the event handler to receive updated channel object with the new notification URI
                this.notificationChannel.ChannelUriUpdated += this.ChannelUriUpdated;
                this.notificationChannel.ErrorOccurred += (s, e) => Deployment.Current.Dispatcher.BeginInvoke(() => this.ErrorOccurred(e));

                // Channel URI will be sent in the event handler after Open is called
                this.notificationChannel.Open();
            }
            else
            {
                // Found an existing channel, now hook up notificaton handlers
                this.HookupNotificationHandlers();

                // Communicate with the app web service for subscribing to notifications
                this.RegisterForNotifications();
            }
        }

        private void RegisterForNotifications()
        {
            string uriStr = this.notificationChannel.ChannelUri.ToString();
            this.encodedUriStr = EncodeTo64(uriStr);
            Debug.WriteLine("Use this channel URI to send notification:" + this.notificationChannel.ChannelUri.ToString());
            string deviceAddUrl = "/register/" + this.deviceIdStr + "?type=WP7&uri=" + this.encodedUriStr;
            this.deviceRestConn.SendPostRequest(deviceAddUrl, string.Empty, "add");
            this.deviceRestConn.UploadHandler += new EventHandler<UploadStringCompletedEventArgs>(this.DeviceRestConnUploadHandler);
        }

        private void DeviceRestConnUploadHandler(object sender, UploadStringCompletedEventArgs e)
        {
            this.SetupSubscriptionOptions();
            this.deviceRestConn.UploadHandler -= new EventHandler<UploadStringCompletedEventArgs>(this.DeviceRestConnUploadHandler);
        }

        private void PushRestConnUploadHandler(object sender, UploadStringCompletedEventArgs e)
        {
            this.SetupSubscriptionOptions();
            this.pushRestConn.UploadHandler -= new EventHandler<UploadStringCompletedEventArgs>(this.PushRestConnUploadHandler);
        }

        private void HookupNotificationHandlers()
        {
            this.notificationChannel.ShellToastNotificationReceived += (s, e) => Deployment.Current.Dispatcher.BeginInvoke(() => this.ToastReceived(e));
            this.notificationChannel.HttpNotificationReceived += (s, e) => Deployment.Current.Dispatcher.BeginInvoke(() => this.HttpNotificationReceived(e));
            this.notificationChannel.ErrorOccurred += (s, e) => Deployment.Current.Dispatcher.BeginInvoke(() => this.ErrorOccurred(e));
        }

        private void HttpNotificationReceived(HttpNotificationEventArgs e)
        {
            var reader = new StreamReader(e.Notification.Body);
            var message = reader.ReadToEnd();
            this.notifications.Items.Add("Raw:" + message);
            reader.Close();
        }

        private void ToastReceived(NotificationEventArgs e)
        {
            this.notifications.Items.Add("Toast:" + e.Collection["wp:Text1"]);
        }

        private void ErrorOccurred(NotificationChannelErrorEventArgs e)
        {
            this.notifications.Items.Add(e.Message);
            Debug.WriteLine("error message :" + e.Message);
        }

        private void ChannelUriUpdated(object sender, NotificationChannelUriEventArgs e)
        {
            // Retrieve the Channel again as the URI is updated
            this.notificationChannel = HttpNotificationChannel.Find(ChannelName);

            // Bind the channel with Toast and Tile 
            if (!this.notificationChannel.IsShellToastBound)
            {
                this.notificationChannel.BindToShellToast();
            }

            if (!this.notificationChannel.IsShellTileBound)
            {
                System.Collections.ObjectModel.Collection<Uri> externalTileUris = new System.Collections.ObjectModel.Collection<Uri> { new Uri(@"http://upload.wikimedia.org/wikipedia/commons/8/8f/MSICO-Start.png") };
                this.notificationChannel.BindToShellTile(externalTileUris);
            }

            // Setup the notification event handlers
            this.HookupNotificationHandlers();

            // Send the device id and the URI to the app notification web service. 
            // Web service should update the URI if it already stored in the web service
            this.RegisterForNotifications();
        }
    }

    // A delegate type for hooking up rest notifications.
    // public delegate void DownloadEventHandler(object sender, DownloadStringCompletedEventArgs e);
    // public delegate void UploadEventHandler(object sender, UploadStringCompletedEventArgs e);

    /// <summary>
    /// Encapsulates the REST protocol to a service
    /// </summary>
    internal class RestConnection
    {
        private string username, password;
        private string baseAddress;

        /// <summary>
        /// Initializes a new instance of the RestConnection class.
        /// </summary>
        /// <param name="tbaseAddress">Base address of the service.</param>
        /// <param name="tusername">User name logging into the service.</param>
        /// <param name="tpassword">Password of the user logging into the service.</param>
        public RestConnection(string tbaseAddress, string tusername, string tpassword)
        {
            this.username = tusername;
            this.password = tpassword;
            this.baseAddress = tbaseAddress;
        }

        /// <summary>
        /// Event handler for downloade completed events.
        /// </summary>
        public event EventHandler<DownloadStringCompletedEventArgs> DownloadHandler;

        /// <summary>
        /// Event handler for Upload completed events.
        /// </summary>
        public event EventHandler<UploadStringCompletedEventArgs> UploadHandler;

        /// <summary>
        /// Use it to send a post request to a REST service.
        /// </summary>
        /// <param name="serviceUrl">Address with respect to the base address.</param>
        /// <param name="postData">Post data string to be added.</param>
        /// <param name="token">Auth Token.</param>
        public void SendPostRequest(string serviceUrl, string postData, string token)
        {
            WebClient pushMsgService = new WebClient();

            pushMsgService.Credentials = new NetworkCredential(this.username, this.password);
            pushMsgService.UploadStringCompleted += new System.Net.UploadStringCompletedEventHandler(this.PushMsgServiceUploadStringCompleted);
            pushMsgService.UploadStringAsync(new Uri(this.baseAddress + serviceUrl), "POST", string.Empty, token);
        }

        /// <summary>
        /// Use to send a Get request to a REST web service. 
        /// </summary>
        /// <param name="serviceUrl">Address with respect to the base address.</param>
        /// <param name="token">Auth Token.</param>
        public void SendGetRequest(string serviceUrl, string token)
        {
            WebClient pushMsgService = new WebClient();
            pushMsgService.Credentials = new NetworkCredential(this.username, this.password);
            pushMsgService.DownloadStringCompleted += new System.Net.DownloadStringCompletedEventHandler(this.PushMsgServiceDownloadStringCompleted);
            pushMsgService.DownloadStringAsync(new Uri(this.baseAddress + serviceUrl), token);
        }

        /// <summary>
        /// Invoke the ResponseHandler event; called whenever we receive the response
        /// </summary>
        /// <param name="e">UploadStringCompletedEventArgs to denote the data received on download.</param>
        protected virtual void OnUploadReceived(UploadStringCompletedEventArgs e)
        {
            if (this.UploadHandler != null)
            {
                this.UploadHandler(this, e);
            }
        }

        /// <summary>
        /// Invoke the ResponseHandler event; called whenever we receive the response 
        /// </summary>
        /// <param name="e">DownloadStringCompletedEventArgs to denote the data received on download.</param>
        protected virtual void OnDownloadReceived(DownloadStringCompletedEventArgs e)
        {
            if (this.DownloadHandler != null)
            {
                this.DownloadHandler(this, e);
            }
        }

        private void PushMsgServiceUploadStringCompleted(object sender, System.Net.UploadStringCompletedEventArgs e)
        {
            try
            {
                this.OnUploadReceived(e);
            }
            catch (WebException)
            {
                // Take appropriate action here
                return;
            }
        }

        private void PushMsgServiceDownloadStringCompleted(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
            try
            {
                this.OnDownloadReceived(e);
            }
            catch (WebException)
            {
                // Take appropriate action here
                return;
            }
        }
    }
}
