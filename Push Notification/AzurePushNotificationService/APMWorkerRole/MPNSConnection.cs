// --------------------------------------------------------------------
// <copyright file="MpnsConnection.cs" company="Microsoft Corp">
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
namespace ApmWorkerRole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Web.Script.Serialization;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;
    using MsgHelperLib.Messages;
    using MsgHelperLib.Model;

    /// <summary>
    /// Message transfer interval definitions. 
    /// </summary>
    internal enum WP7BatchingInterval
    {
        /// <summary>
        /// Dummy element - not used. 
        /// </summary>
        None = 0,

        /// <summary>
        /// Send the tile immmediately.
        /// </summary>
        TileImmediately = 1,

        /// <summary>
        /// Send the toast immmediately.
        /// </summary>
        ToastImmediately = 2,

        /// <summary>
        /// Send the raw immmediately.
        /// </summary>
        RawImmediately = 3,

        /// <summary>
        /// Send the tile in 450 ms.
        /// </summary>
        TileWait450 = 11,

        /// <summary>
        /// Send the toast in 450 ms.
        /// </summary>
        ToastWait450 = 12,

        /// <summary>
        /// Send the raw in 450 ms.
        /// </summary>
        RawWait450 = 13,

        /// <summary>
        /// Send the tile in 900 ms.
        /// </summary>
        TileWait900 = 21,

        /// <summary>
        /// Send the toast in 900 ms.
        /// </summary>
        ToastWait900 = 22,

        /// <summary>
        /// Send the raw in 900 ms.
        /// </summary>
        RawWait900 = 23
    }

    /// <summary>
    /// Class to handle connection to Microsoft Push Notification Service
    /// </summary>
    internal class MpnsConnection : Connection
    {
        #region Constants and private variables

        // Definitions to various datasources. These are tables in Azure table storage
        private WP7BatchingPolicy batchingPolicy;
        private SubscriptionDataSource sds;
        private int retries;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the MpnsConnection class.
        /// </summary>
        /// <param name="batchingPolicyParam">The batching policy to use.</param>
        /// <param name="retriesParam">Number of connection and send retries. </param>
        public MpnsConnection(WP7BatchingPolicy batchingPolicyParam, int retriesParam)
        {
            this.batchingPolicy = batchingPolicyParam;
            this.retries = retriesParam;
            this.sds = new SubscriptionDataSource();
        }

        /// <summary>
        /// Cleaup the connection state.
        /// </summary>
        public override void Close()
        {
            return;
        }
        #endregion

        #region Delegates and Events

        /// <summary>
        /// Event handler for Device Id format error.
        /// </summary>
        public override event EventHandler<NotificationEventArgs> DeviceIdFormatError;

        /// <summary>
        /// Event handler for the Notification format error.
        /// </summary>
        public override event EventHandler<NotificationEventArgs> NotificationFormatError;

        /// <summary>
        /// Event handler for Notification error.
        /// </summary>
        public override event EventHandler<NotificationEventArgs> NotificationError;

        /// <summary>
        /// Event handler for the notification failed. 
        /// </summary>
        public override event EventHandler<NotificationEventArgs> NotificationFailed;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the device type supported by the MpnsConnection.
        /// </summary>
        public override string SupportedDeviceType
        {
            get
            {
                return "WP7";
            }
        }

        // Gets the toast interval using current policy 
        private WP7BatchingInterval ToastInterval
        {
            get
            {
                if (this.batchingPolicy == WP7BatchingPolicy.Wait450)
                {
                    return WP7BatchingInterval.ToastWait450;
                }
                else if (this.batchingPolicy == WP7BatchingPolicy.Wait900)
                {
                    return WP7BatchingInterval.ToastWait900;
                }
                else
                {
                    return WP7BatchingInterval.ToastImmediately;
                }
            }
        }

        // Gets the tile interval using current policy.
        private WP7BatchingInterval TileInterval
        {
            get
            {
                if (this.batchingPolicy == WP7BatchingPolicy.Wait450)
                {
                    return WP7BatchingInterval.TileWait450;
                }
                else if (this.batchingPolicy == WP7BatchingPolicy.Wait900)
                {
                    return WP7BatchingInterval.TileWait900;
                }
                else
                {
                    return WP7BatchingInterval.TileImmediately;
                }
            }
        }

        // Gets the raw message interval using current policy.
        private WP7BatchingInterval RawInterval
        {
            get
            {
                if (this.batchingPolicy == WP7BatchingPolicy.Wait450)
                {
                    return WP7BatchingInterval.RawWait450;
                }
                else if (this.batchingPolicy == WP7BatchingPolicy.Wait900)
                {
                    return WP7BatchingInterval.RawWait900;
                }
                else
                {
                    return WP7BatchingInterval.RawImmediately;
                }
            }
        }

        /// <summary>
        /// Provides the messages this connection handles.
        /// </summary>
        /// <param name="messageType">The message type to be evaluated.</param>
        /// <returns>Returns true if the MpnsConnection handles the specific message type, false otherwise.</returns>
        public override bool HandlesMessageType(PushMessageType messageType)
        {
            switch (messageType)
            {
                case PushMessageType.Iphone:
                    return false;
                case PushMessageType.Common:
                    return true;
                case PushMessageType.Raw:
                    return true;
                case PushMessageType.Tile:
                    return true;
                case PushMessageType.Toast:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Enque either a common or WP7 message to be sent to MPNS. Get message components and send it to the device
        /// </summary>
        /// <param name="device">Device to send the message to</param>
        /// <param name="msg">Message to send</param>
        public override void EnqueMessage(DeviceDataModel device, PushMessage msg)
        {
            if (msg.MessageType == (short)PushMessageType.Toast)
            {
                this.EnqueWP7ToastNotification(device, msg.Message["toast"]);
            }
            else if (msg.MessageType == (short)PushMessageType.Raw)
            {
                this.EnqueWP7RawNotification(device, msg.Message["message"]);
            }
            else if (msg.MessageType == (short)PushMessageType.Tile || msg.MessageType == (short)PushMessageType.Common)
            {
                this.EnqueWP7TileNotification(device, msg.Message["title"], int.Parse(msg.Message["count"], CultureInfo.InvariantCulture), msg.Message["url"]);
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Enque either a common or WP7 message to be sent to MPNS. Get message components and send it to the device
        /// </summary>
        /// <param name="devices">Devices to send the message to</param>
        /// <param name="msg">Message to send</param>
        public override void EnqueMessage(IEnumerable<DeviceDataModel> devices, PushMessage msg)
        {
            try
            {
                if (msg.MessageType == (short)PushMessageType.Toast)
                {
                    this.EnqueWP7ToastNotification(devices, msg.Message["toast"]);
                }
                else if (msg.MessageType == (short)PushMessageType.Raw)
                {
                    this.EnqueWP7RawNotification(devices, msg.Message["raw"]);
                }
                else if (msg.MessageType == (short)PushMessageType.Tile || msg.MessageType == (short)PushMessageType.Common)
                {
                    this.EnqueWP7TileNotification(devices, msg.Message["title"], int.Parse(msg.Message["count"], CultureInfo.InvariantCulture), msg.Message["url"]);
                }
                else
                {
                    return;
                }
            }
            catch (Exception)
            {
                if (this.NotificationFormatError != null)
                {
                    this.NotificationFormatError(this, new NotificationEventArgs(new NotificationFormatException(msg)));
                }
            }
        }

        #endregion

        #region Private Methods

        private static byte[] PackageToastNotification(string message)
        {
            string toastMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                 "<wp:Notification xmlns:wp=\"WPNotification\">" +
                    "<wp:Toast>" +
                       "<wp:Text1>{0}</wp:Text1>" +
                      "</wp:Toast>" +
                 "</wp:Notification>";
            string formattedToastMessage = string.Format(CultureInfo.InvariantCulture, toastMessage, message);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(formattedToastMessage);
            return messageBytes;
        }

        private static byte[] PackageTileNotification(string title, int count, string image)
        {
            string tileMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<wp:Notification xmlns:wp=\"WPNotification\">" +
                   "<wp:Tile>" +
                      "<wp:BackgroundImage>{0}</wp:BackgroundImage>" +
                      "<wp:Count>{1}</wp:Count>" +
                      "<wp:Title>{2}</wp:Title>" +
                   "</wp:Tile> " +
                "</wp:Notification>";
            string formattedTileMessage = string.Format(CultureInfo.InvariantCulture, tileMessage, image, count, title);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(formattedTileMessage);
            return messageBytes;
        }

        /// <summary>
        /// Formats a toast message and enques it for sending it out a device. 
        /// </summary>
        /// <param name="ddm">The device to send the message to.</param>
        /// <param name="message">The message to send.</param>
        private void EnqueWP7ToastNotification(DeviceDataModel ddm, string message)
        {
            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "WCF Worker role: Enque Toast for deviceID : {0}", ddm.DeviceId));
            byte[] messageBytes = PackageToastNotification(message);
            this.EnqueWP7Notification(ddm, messageBytes, WP7NotificationType.Toast);
        }

        private void EnqueWP7ToastNotification(IEnumerable<DeviceDataModel> ddmList, string message)
        {
            byte[] messageBytes = PackageToastNotification(message);
            this.EnqueWP7Notification(ddmList, messageBytes, WP7NotificationType.Toast);
        }

        /// <summary>
        /// Enques all types of WP7 message to each device URI in the subscription.       
        /// </summary>
        /// <param name="ddm">Device to send message to.</param>
        /// <param name="messageBytes">Message bytes of the message to send.</param>
        /// <param name="type">Type of message to send.</param>
        private void EnqueWP7Notification(DeviceDataModel ddm, byte[] messageBytes, WP7NotificationType type)
        {
            // Get the all devices that have signed up for the subscription
            // Send the message to each device addrress or URI
            if (Uri.IsWellFormedUriString(ddm.Address, UriKind.Absolute))
            {
                this.SendWP7Message(new Uri(ddm.Address), messageBytes, type);
            }
            else
            {
                if (this.DeviceIdFormatError != null)
                {
                    this.DeviceIdFormatError(this, new NotificationEventArgs(new DeviceIdFormatException(ddm.Address)));
                }
            }
        }

        private void EnqueWP7Notification(IEnumerable<DeviceDataModel> ddmList, byte[] messageBytes, WP7NotificationType type)
        {
            // Get the all devices that have signed up for the subscription
            // Send the message to each device addrress or URI
            foreach (DeviceDataModel ddm in ddmList)
            {
                if (Uri.IsWellFormedUriString(ddm.Address, UriKind.Absolute))
                {
                    try
                    {
                        this.SendWP7Message(new Uri(ddm.Address), messageBytes, type);
                    }
                    catch (WebException e)
                    {
                        HttpWebResponse wr = e.Response as HttpWebResponse;
                        if (wr.StatusCode == HttpStatusCode.NotFound)
                        {
                            this.sds.DeleteDeviceSubscriptions(ddm);
                        }
                    }
                }
                else
                {
                    if (this.DeviceIdFormatError != null)
                    {
                        this.DeviceIdFormatError(this, new NotificationEventArgs(new DeviceIdFormatException(ddm.Address)));
                    }
                }
            }
        }

        /// <summary>
        /// Formats a raw message and enques it for sending it out to all devices in the subscription. 
        /// </summary>
        /// <param name="ddm">Device to send a raw message.</param>
        /// <param name="message">Message to send.</param>
        private void EnqueWP7RawNotification(DeviceDataModel ddm, string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            this.EnqueWP7Notification(ddm, messageBytes, WP7NotificationType.Raw);
        }

        /// <summary>
        /// Formats a raw message and enques it for sending it out to all devices in the subscription. 
        /// </summary>
        /// <param name="ddmList">List of devices to send a raw message.</param>
        /// <param name="message">Message to send.</param>
        private void EnqueWP7RawNotification(IEnumerable<DeviceDataModel> ddmList, string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            this.EnqueWP7Notification(ddmList, messageBytes, WP7NotificationType.Raw);
        }

        /// <summary>
        /// Formats a file message and enques it for sending it out to  dtheevices in the subscription. 
        /// </summary>
        /// <param name="ddm">Devices to send the message to.</param>
        /// <param name="title">Title of the push notification.</param>
        /// <param name="count">Count to be shown on the tile.</param>
        /// <param name="image">Image to show on the tile.</param>
        private void EnqueWP7TileNotification(DeviceDataModel ddm, string title, int count, string image)
        {
            byte[] messageBytes = PackageTileNotification(title, count, image);
            this.EnqueWP7Notification(ddm, messageBytes, WP7NotificationType.Tile);
        }

        /// <summary>
        /// Formats a file message and enques it for sending it out to all devices in the subscription. 
        /// </summary>
        /// <param name="ddmList">List of devices to send the message to.</param>
        /// <param name="title">Title of the push notification.</param>
        /// <param name="count">Count to be shown on the tile.</param>
        /// <param name="image">Image to show on the tile.</param>
        private void EnqueWP7TileNotification(IEnumerable<DeviceDataModel> ddmList, string title, int count, string image)
        {
            byte[] messageBytes = PackageTileNotification(title, count, image);
            this.EnqueWP7Notification(ddmList, messageBytes, WP7NotificationType.Tile);
        }

        /// <summary>
        /// Send the message out to the device URI.
        /// </summary>
        /// <param name="uri">URI of the device to send message to.</param>
        /// <param name="messageBytes">Bytes of the message.</param>
        /// <param name="notificationType">Type of the notification - Tile, Toast or Raw.</param>
        private void SendWP7Message(Uri uri, byte[] messageBytes, WP7NotificationType notificationType)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "text/xml";
            request.ContentLength = messageBytes.Length;
            request.Headers.Add("X-MessageID", Guid.NewGuid().ToString());

            // If the cloud service sending this request is authenticated, it needs to send its certificate.  
            // Otherwise, this step is not needed.  
            // if (_x509Certificate != null)
            //    request.ClientCertificates.Add(_x509Certificate);  
            switch (notificationType)
            {
                case WP7NotificationType.Toast:
                    request.Headers["X-WindowsPhone-Target"] = "toast";
                    request.Headers.Add("X-NotificationClass", ((int)this.ToastInterval).ToString(CultureInfo.InvariantCulture));
                    break;
                case WP7NotificationType.Tile:
                    request.Headers["X-WindowsPhone-Target"] = "token";
                    request.Headers.Add("X-NotificationClass", ((int)this.TileInterval).ToString(CultureInfo.InvariantCulture));
                    break;
                case WP7NotificationType.Raw:
                    request.Headers.Add("X-NotificationClass", ((int)this.RawInterval).ToString(CultureInfo.InvariantCulture));
                    break;
            }

            bool sent = false;
            int tries = 0;
            HttpWebResponse response;

            while (!sent && tries < this.retries)
            {
                try
                {
                    using (var requestStream = request.GetRequestStream())
                    {
                        // we are not using retries yet.. or looking for errors here. We should log errors too.
                        requestStream.Write(messageBytes, 0, messageBytes.Length);
                    }

                    // Sends the notification and gets the response.
                    response = (HttpWebResponse)request.GetResponse();
                    string notificationStatus = response.Headers["X-NotificationStatus"];
                    string notificationChannelStatus = response.Headers["X-SubscriptionStatus"];
                    string deviceConnectionStatus = response.Headers["X-DeviceConnectionStatus"];
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return;
                    }

                    if (deviceConnectionStatus == "TempDisconnected" || notificationStatus == "QueueFull")
                    {
                        tries++;
                        if (tries > this.retries)
                        {
                            if (this.NotificationFailed != null)
                            {
                                this.NotificationFailed(this, new NotificationEventArgs(new NotificationException(string.Format(CultureInfo.InvariantCulture, "{0} notification failures for {1}. Giving up", this.retries, uri.OriginalString))));
                            }

                            return;
                        }
                    }
                    else
                    {
                        sent = true;
                    }
                }
                catch (WebException e)
                {
                    // Check why we got the exception
                    if (this.NotificationFailed != null)
                    {
                        this.NotificationFailed(this, new NotificationEventArgs(new NotificationException("Notification failed with exception " + e.Message + "for " + uri.OriginalString, e)));
                    }

                    throw;
                }
            }
        }

        #endregion
    }
}
