// --------------------------------------------------------------------
// <copyright file="C2dmConnection.cs" company="Microsoft Corp">
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
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;
    using System.Web.Script.Serialization;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;
    using MsgHelperLib.Model;
    using MsgHelperLib.Messages;

    /// <summary>
    /// Class to handle connection to Cloud to Device Messaging (Google / Android Push Notification Service)
    /// </summary>
    internal class C2dmConnection : Connection
    {
        #region Constants and private variables

        /// <summary>
        /// Key used for sending device registration to C2dm
        /// </summary>
        private const string RegId = "registration_id";

        /// <summary>
        /// Key used to collapse similar messages when device is offline.
        /// </summary>
        private const string CollapseKey = "collapse_key";

        /// <summary>
        /// c2dm URL address.
        /// </summary>
        private const string C2dmUrlStr = "https://android.clients.google.com/c2dm/send";

        private const string Username = "vivek_nirkhe@hotmail.com";
        private const string Password = "hyder*89";

        // definitions to various datasources. These are tables in Azure table storage
        private DeviceDataSource dds;
        private SubscriptionDataSource sds;

        private string authKey; // Google ClientLogin authorization token sent in C2DM. 

        private int numOfRetries;
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the C2dmConnection class.
        /// </summary>
        /// <param name="retries"></param>
        public C2dmConnection(int retries)
        {
            this.numOfRetries = retries;
            this.sds = new SubscriptionDataSource();
            this.dds = new DeviceDataSource();
            if (this.authKey == null)
            {
                //Uncomment this after registering an account and configuring Google C2DM 
                //this.Login(Username, Password);
            }
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

        /// <summary>
        /// Gets the device type supported by C2dmconnection
        /// </summary>
        public override string SupportedDeviceType 
        {
            get 
            { 
                return "Android"; 
            } 
        }

        #region Public Methods

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
                    return true;
                case PushMessageType.Common:
                    return true;
                case PushMessageType.Raw:
                    return true;
                case PushMessageType.Tile:
                    return false;
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
                this.EnqueAndroidToastNotification(device, msg.Message["toast"]);
            }
            else if (msg.MessageType == (short)PushMessageType.Raw)
            {
                this.EnqueAndroidRawNotification(device, msg.Message["raw"]);
            }
            else if (msg.MessageType == (short)PushMessageType.Common)
            {
                this.EnqueAndroidCommonNotification(device, msg.Message["title"], int.Parse(msg.Message["count"], CultureInfo.InvariantCulture), msg.Message["sound"]);
            }
            else if (msg.MessageType == (short)PushMessageType.Iphone)
            {
                this.EnqueAndroidCommonNotification(device, msg.Message["title"], int.Parse(msg.Message["count"], CultureInfo.InvariantCulture), msg.Message["sound"]);
            }
            else
            {
                return;
            }
        }

        public static bool C2dmCertValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;

            //// To tighten cert validation, uncomment following code. 
            /*
            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors)
                      == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                return true;
            }
            else if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) 
                            == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                Zone z;
                z = Zone.CreateFromUrl(((HttpWebRequest)sender).RequestUri.ToString());
                return true;
            }
            return false;
             */
        }

        /// <summary>
        /// Close the connections
        /// </summary>
        public override void Close()
        {
            return;
        }

        #endregion

        #region Private Methods

        private static byte[] PackageToastNotification(string deviceID, string message)
        {
            StringBuilder postMsgBuilder = new StringBuilder();
            postMsgBuilder.Append(RegId).Append("=").Append(deviceID);
            postMsgBuilder.Append("&").Append(CollapseKey).Append("=").Append("0");
            postMsgBuilder.Append("&").Append("data.message=").Append(message);
            postMsgBuilder.Append("&").Append("data.type=").Append("toast");
            byte[] messageBytes = Encoding.UTF8.GetBytes(postMsgBuilder.ToString());
            return messageBytes;
        }

        private static byte[] PackageRawNotification(string deviceID, string message)
        {
            StringBuilder postMsgBuilder = new StringBuilder();
            postMsgBuilder.Append(RegId).Append("=").Append(deviceID);
            postMsgBuilder.Append("&").Append(CollapseKey).Append("=").Append("0");
            postMsgBuilder.Append("&").Append("data.message=").Append(message);
            postMsgBuilder.Append("&").Append("data.type=").Append("raw");
            byte[] messageBytes = Encoding.UTF8.GetBytes(postMsgBuilder.ToString());
            return messageBytes;
        }

        /// <summary>
        /// formats a toast message and enques it for sending it out to all devices in the subscription. 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="message"></param>
        private void EnqueAndroidToastNotification(DeviceDataModel ddm, string message)
        {
            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "WCF Worker role: Enque Toast for deviceID : {0}", ddm.DeviceId));
            byte[] formattedMsgBytes = PackageToastNotification(ddm.DeviceId, message);
            this.EnqueAndroidNotification(ddm, formattedMsgBytes);
        }

        private byte[] PackageCommonNotification(string deviceID, string title, int count, string sound)
        {
            StringBuilder postMsgBuilder = new StringBuilder();
            postMsgBuilder.Append(RegId).Append("=").Append(deviceID);
            postMsgBuilder.Append("&").Append(CollapseKey).Append("=").Append("0");
            postMsgBuilder.Append("&").Append("data.message=").Append(title);
            postMsgBuilder.Append("&").Append("data.sound=").Append(sound);
            postMsgBuilder.Append("&").Append("data.count=").Append(count.ToString(CultureInfo.InvariantCulture));
            postMsgBuilder.Append("&").Append("data.type=").Append("common");
            byte[] messageBytes = Encoding.UTF8.GetBytes(postMsgBuilder.ToString());
            return messageBytes;
        }

        /// <summary>
        ///  enques all types of WP7 message to each device URI in the subscription.       
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="messageBytes"></param>
        /// <param name="type"></param>
        private void EnqueAndroidNotification(DeviceDataModel ddm, byte[] msgBytes)
        {
            try
            {
                // get the all devices that have signed up for the subscription
                // send the message to each device addrress or URI
                this.SendAndroidMessage(msgBytes);
            }
            catch (ObjectDisposedException e)
            {
                if (this.NotificationFailed != null)
                {
                    this.NotificationFailed(this, new NotificationEventArgs(e));
                }
            }
            catch (System.IO.IOException e)
            {
                if (this.NotificationFailed != null)
                {
                    this.NotificationFailed(this, new NotificationEventArgs(e));
                }
            }
        }

        /// <summary>
        /// formats a raw message and enques it for sending it out to all devices in the subscription. 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="message"></param>
        private void EnqueAndroidRawNotification(DeviceDataModel ddm, string message)
        {
            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "WCF Worker role: Enque Raw for deviceID : {0}", ddm.DeviceId));
            byte[] formattedMsgBytes = PackageRawNotification(ddm.DeviceId, message);
            this.EnqueAndroidNotification(ddm, formattedMsgBytes);
        }

        /// <summary>
        /// formats a file message and enques it for sending it out to all devices in the subscription. 
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="title"></param>
        /// <param name="count"></param>
        /// <param name="image"></param>
        private void EnqueAndroidCommonNotification(DeviceDataModel ddm, string title, int count, string sound)
        {
            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "WCF Worker role: Enque Raw for deviceID : {0}", ddm.DeviceId));
            byte[] formattedMsgBytes = this.PackageCommonNotification(ddm.DeviceId, title, count, sound);
            this.EnqueAndroidNotification(ddm, formattedMsgBytes);
        }

        /// <summary> 
        /// Simple client login. 
        /// </summary> 
        /// <param name="username"></param> 
        /// <param name="password"></param> 
        private void Login(string username, string password)
        {
            string sreq = "https://www.google.com/accounts/ClientLogin? accountType=GOOGLE&Email=" + username + "&Passwd=" + password + "&service=ac2dm&source=AzurePushSample";
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(C2dmCertValidationCallback);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sreq);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream responseBody = res.GetResponseStream();
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader readStream = new StreamReader(responseBody, encode);
            string loginStuff = readStream.ReadToEnd();
            this.authKey = loginStuff.Substring(loginStuff.IndexOf("Auth")).Replace("Auth=", string.Empty).TrimEnd('\n');
        } 

        /// <summary>
        /// send the message out to the device URI
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="messageBytes"></param>
        /// <param name="notificationType"></param>
        private void SendAndroidMessage(byte[] messageBytes)
        {
            if (this.authKey == null)
            {
                //Uncomment this after registering an account and configuring Google C2DM 
                //this.Login(Username, Password);
            }

            var request = (HttpWebRequest)WebRequest.Create(C2dmUrlStr);

            // Hook a callback to verify the remote certificate
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(C2dmCertValidationCallback);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            request.ContentLength = messageBytes.Length;
            request.Headers.Add("Authorization", "GoogleLogin auth=" + this.authKey);

            // If the cloud service sending this request is authenticated, it needs to send its certificate.  
            // Otherwise, this step is not needed.  
            // if (_x509Certificate != null)
            //    request.ClientCertificates.Add(_x509Certificate);  
            bool sent = false;
            int tries = 0;
            HttpWebResponse response;
            while (!sent && tries < this.numOfRetries)
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
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return;
                    }

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        tries++;
                        if (tries > this.numOfRetries)
                        {
                            if (this.NotificationFailed != null)
                            {
                                this.NotificationFailed(this, new NotificationEventArgs(new NotificationException(string.Format(CultureInfo.InvariantCulture, "{0} notification failures for {1}. Giving up", this.numOfRetries, C2dmUrlStr))));
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
                    // check why we got the exception 
                    if (this.NotificationFailed != null)
                    {
                        this.NotificationFailed(this, new NotificationEventArgs(new NotificationException("Notification failed with exception " + e.Message + "for " + C2dmUrlStr, e)));
                    }
                    
                    throw;
                }
            }
        }
        #endregion
    }
}
