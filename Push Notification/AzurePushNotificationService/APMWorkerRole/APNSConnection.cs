// --------------------------------------------------------------------
// <copyright file="ApnsConnection.cs" company="Microsoft Corp">
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
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Web.Script.Serialization;
    using MsgHelperLib.Helpers;
    using MsgHelperLib.Messages;
    using MsgHelperLib.Model;

    /// <summary>
    /// Class that is used to connect to Apple Notification Service.
    /// It sends notification messages to APNS which forwards them to the real device
    /// </summary>
    internal class ApnsConnection : Connection, IDisposable
    {
        #region Constants
        private const string HostSandbox = "gateway.sandbox.push.apple.com";
        private const string HostProduction = "gateway.push.apple.com";

        private const int DeviceTokenBinarySize = 32;
        private const int DeviceTokenStringSize = 64;
        private const int MaxPayloadSize = 256;

        #endregion

        #region Instance Variables
        private bool isConnected;
        private bool disposed = false;

        // private Encoding encoding = Encoding.ASCII;
        private X509Certificate certificate;
        private X509CertificateCollection certificates;
        private TcpClient apnsTCPClient;
        private SslStream apnsSecureStream;
        private int connectionRetries;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ApnsConnection class
        /// </summary>
        /// <param name="sandbox">Boolean flag indicating whether the default Sandbox or Production Host and Port should be used</param>
        /// <param name="certificateFileName">PKCS12 .p12 or .pfx File containing Public and Private Keys</param>
        /// <param name="certificatePassword">Password protecting the p12File</param>
        /// <param name="retries">Number of connection and send retries for the message </param>
        public ApnsConnection(bool sandbox, string certificateFileName, string certificatePassword, int retries)
        {
            this.isConnected = false;
            this.Host = sandbox ? HostSandbox : HostProduction;
            this.Port = 2195;

            this.SendRetries = retries;
            this.connectionRetries = retries;
            this.InitializeCertificate(certificateFileName, certificatePassword);
        }
        #endregion

        #region Delegates and Events
        /// <summary>
        /// Event handler for Device Id error. 
        /// </summary>
        public override event EventHandler<NotificationEventArgs> DeviceIdFormatError;

        /// <summary>
        /// Event handler if the notification message format is in correct
        /// </summary>
        public override event EventHandler<NotificationEventArgs> NotificationFormatError;

        /// <summary>
        /// Event handler if there was error in other notification errors
        /// </summary>
        public override event EventHandler<NotificationEventArgs> NotificationError;

        /// <summary>
        /// Event handler if failure in sending notification, server errors etc.
        /// </summary>
        public override event EventHandler<NotificationEventArgs> NotificationFailed;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the Number of times to try resending a Notification before the NotificationFailed event is raised
        /// </summary>
        public int SendRetries
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Push Notification Gateway Host
        /// </summary>
        public string Host
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Push Notification Gateway Port
        /// </summary>
        public int Port
        {
            get;
            private set;
        }

        /// <summary>
        /// This property provides the Device Type supported by the connection class.
        /// </summary>
        public override string SupportedDeviceType
        {
            get
            {
                return "iOS";
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Return the types of messages supported by APNS. Support common or iphone message types.
        /// Returns true if ApnsConnection class handles a particular message type. False if it does not support a particular type
        /// </summary>
        /// <param name="messageType">type of the message to check</param>
        /// <returns>Returns a boolean indicating whether a given messageType is supported</returns>
        public override bool HandlesMessageType(PushMessageType messageType)
        {
            switch (messageType)
            {
                case PushMessageType.Iphone:
                    return true;
                case PushMessageType.Common:
                    return true;
                case PushMessageType.Raw:
                    return false;
                case PushMessageType.Tile:
                    return false;
                case PushMessageType.Toast:
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Enque either a common or iPhone message type to be sent to APNS. Get message components and enque it for the device
        /// </summary>
        /// <param name="device">Device to send the message to</param>
        /// <param name="msg">Message to send</param>
        public override void EnqueMessage(DeviceDataModel device, PushMessage msg)
        {
            if (msg.MessageType == (short)PushMessageType.Iphone)
            {
                string msgStr = ToString(msg);
                iPhoneMessage iPhoneMsg = FromString(msgStr, typeof(iPhoneMessage)) as iPhoneMessage;
                this.EnqueiOSMessage(device, iPhoneMsg);
            }
            else if (msg.MessageType == (short)PushMessageType.Common)
            {
                string msgStr = ToString(msg);
                iPhoneMessage iPhoneMsg = FromString(msgStr, typeof(iPhoneMessage)) as iPhoneMessage;
                this.EnqueiOSMessage(device, iPhoneMsg);
            }
        }

        /// <summary>
        /// This method is used for IDisposable. 
        /// Since apnsSecureStream and apnsTCPClient support IDisposable, apnsConnection must support IDisposable
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.apnsSecureStream != null)
                    {
                        this.apnsSecureStream.Dispose();
                    }

                    if (this.apnsTCPClient != null)
                    {
                        this.apnsTCPClient.Close();
                    }
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Cleanup the state.
        /// </summary>
        public override void Close()
        {
            if (this.apnsSecureStream != null)
            {
                this.DisconnectTCPClient();
                this.apnsSecureStream = null;
                this.isConnected = false;
            }

            if (this.apnsTCPClient != null)
            {
                this.CloseSecureStream();
                this.apnsSecureStream = null;
                this.isConnected = false;
            }

        }

        #region Private Methods

        /// <summary>
        /// We check that the device ID is legal again. Higher layers may ave already done but at worker role, we shold not rely on it
        /// </summary>
        /// <param name="deviceId">deviceId to check</param>
        /// <returns>Returns a bool indicating if the device is legal</returns>
        private static bool CheckDeviceId(string deviceId)
        {
            byte[] deviceToken = new byte[deviceId.Length / 2];
            for (int i = 0; i < deviceToken.Length; i++)
            {
                deviceToken[i] = byte.Parse(deviceId.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            if (deviceToken.Length != DeviceTokenBinarySize)
            {
                return false;
            }

            return true;
        }

        // uses .NET serializer to serialize the message. 
        // here is how it should look like {"aps": {"badge":3, "alert":"This is my alert", "sound":"default"}}
        private static string ToJson(iPhoneMessage message)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            AppleNotification an = new AppleNotification();
            if (!string.IsNullOrEmpty(message.Alert))
            {
                an.Aps.Alert = message.Alert;
            }
            else
            {
                an.Aps.Alert = string.Empty;
            }

            if (!string.IsNullOrEmpty(message.Badge))
            {
                an.Aps.Badge = int.Parse(message.Badge, CultureInfo.InvariantCulture);
            }

            if (!string.IsNullOrEmpty(message.Sound))
            {
                an.Aps.Sound = message.Sound;
            }
            else
            {
                an.Aps.Sound = string.Empty;
            }

            string str = js.Serialize(an);
            return str;
        }

        /// <summary>
        /// deserialize from json
        /// </summary>
        /// <param name="str"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        private static object FromString(string str, System.Type objectType)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            return jser.Deserialize(str, objectType);
        }

        /// <summary>
        /// serialize to json
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static string ToString(PushMessage o)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            string contents = jser.Serialize(o);
            return contents;
        }

        /// <summary>
        /// add the new SSL cert 
        /// </summary>
        /// <param name="p12File"></param> 
        /// <param name="p12FilePassword"></param>
        private void InitializeCertificate(string p12File, string p12FilePassword)
        {
            // Need to load the private key seperately from apple
            if (string.IsNullOrEmpty(p12FilePassword))
            {
                this.certificate = new X509Certificate2(System.IO.File.ReadAllBytes(p12File));
            }
            else
            {
                //Uncomment the following after adding APNS X.509 certificate 
                //this.certificate = new X509Certificate2(System.IO.File.ReadAllBytes(p12File), p12FilePassword);
            }

            this.certificates = new X509CertificateCollection();
            //Uncomment the following after adding APNS X.509 certificate 
            //this.certificates.Add(this.certificate);
        }

        /// <summary>
        /// Compose the APNS message per format . Look up Apple's docs. This packs the message in a binary format. 
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private byte[] ComposeAPNSMessage(string deviceID, iPhoneMessage message)
        {
            // get byte size of deviceID
            byte[] deviceIDBytes = new byte[deviceID.Length / 2];

            // get deviceID bytes
            for (int i = 0; i < deviceIDBytes.Length; i++)
            {
                deviceIDBytes[i] = byte.Parse(deviceID.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            if (deviceIDBytes.Length != DeviceTokenBinarySize)
            {
                throw new DeviceIdFormatException(deviceID);
            }

            // size is always 32
            byte[] deviceIDSize = new byte[2] { 0, 32 };
            string messageInJson = ToJson(message).Replace("Aps", "aps").Replace("Alert", "alert").Replace("Badge", "badge").Replace("Sound", "sound"); 
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageInJson);

            // if the message is longer, trim the alert
            if (messageBytes.Length > MaxPayloadSize)
            {
                int newSize = message.Alert.Length - (messageBytes.Length - MaxPayloadSize);
                if (newSize > 0)
                {
                    message.Alert = message.Alert.Substring(0, newSize);
                    messageBytes = Encoding.UTF8.GetBytes(message.ToString());
                }
            }

            // convert the message to binary
            byte[] messageBytesSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(messageBytes.Length)));

            // find the titak buffer size
            int bufferSize = sizeof(byte) + deviceIDSize.Length + deviceIDBytes.Length + messageBytesSize.Length + messageBytes.Length;
            byte[] apnsMessage = new byte[bufferSize];

            // pack the message
            apnsMessage[0] = 0x00;
            Buffer.BlockCopy(deviceIDSize, 0, apnsMessage, sizeof(byte), deviceIDSize.Length);
            Buffer.BlockCopy(deviceIDBytes, 0, apnsMessage, sizeof(byte) + deviceIDSize.Length, deviceIDBytes.Length);
            Buffer.BlockCopy(messageBytesSize, 0, apnsMessage, sizeof(byte) + deviceIDSize.Length + deviceIDBytes.Length, messageBytesSize.Length);
            Buffer.BlockCopy(messageBytes, 0, apnsMessage, sizeof(byte) + deviceIDSize.Length + deviceIDBytes.Length + messageBytesSize.Length, messageBytes.Length);

            return apnsMessage;
        }

        /// <summary>
        /// Enque message for a particular device. For iPhone, we have to pack each message separately as the device ID is put in the message
        /// </summary>
        /// <param name="device"></param>
        /// <param name="msg"></param>
        private void EnqueiOSMessage(DeviceDataModel device, iPhoneMessage msg)
        {
            byte[] apnsNotificationBytes;
            int numberOfRetries = 0;
            if (device.DeviceType == "iOS")
            {
                if (!CheckDeviceId(device.DeviceId))
                {
                    return;
                }

                // compose the message
                apnsNotificationBytes = this.ComposeAPNSMessage(device.DeviceId, msg as iPhoneMessage);
                while (numberOfRetries < this.SendRetries)
                {
                    try
                    {
                        // and send the notification
                        this.SendiPhoneDeviceNotification(apnsNotificationBytes);
                        return;
                    }
                    catch (DeviceIdFormatException e)
                    {
                        // device ID was wrong. Send an error to listener
                        if (this.DeviceIdFormatError != null)
                        {
                            this.DeviceIdFormatError(this, new NotificationEventArgs(e));
                        }
                    }
                    catch (NotificationFormatException e)
                    {
                        // notification format was wrong. send error to listner
                        if (this.NotificationFormatError != null)
                        {
                            this.NotificationFormatError(this, new NotificationEventArgs(e));
                        }
                    }
                    catch (ObjectDisposedException e)
                    {
                        if (this.NotificationFailed != null)
                        {
                            this.NotificationFailed(this, new NotificationEventArgs(e));
                        }

                        this.DisconnectTCPClient();
                    }
                    catch (System.IO.IOException e)
                    {
                        if (this.NotificationFailed != null)
                        {
                            this.NotificationFailed(this, new NotificationEventArgs(e));
                        }

                        this.DisconnectTCPClient();
                    }

                    numberOfRetries++;
                }
            }
        }

        /// <summary>
        /// Send notification to the APNS service
        /// </summary>
        /// <param name="apnsMessageBytes"></param>
        private void SendiPhoneDeviceNotification(byte[] apnsMessageBytes)
        {
            int connectionAttempts = 0;
            while (connectionAttempts < this.SendRetries && !this.isConnected)
            {
                // if we are not connected, make an SSL connection
                connectionAttempts++;
                this.ConnectSecureConnection();
            }

            // once connected write to the SSL connection
            if (this.isConnected)
            {
                this.apnsSecureStream.Write(apnsMessageBytes);
                this.apnsSecureStream.Flush();
                return;
            }
        }

        /// <summary>
        /// Make an SSL connection to APNS
        /// </summary>
        /// <returns></returns>
        private bool ConnectSecureConnection()
        {
            if (this.apnsSecureStream != null && this.apnsSecureStream.CanWrite)
            {
                this.DisconnectTCPClient();
                this.apnsSecureStream = null;
                this.isConnected = false;
            }

            if (this.apnsTCPClient != null && this.apnsTCPClient.Connected)
            {
                this.CloseSecureStream();
                this.apnsSecureStream = null;
                this.isConnected = false;
            }

            // Try connecting
            if (this.ConnectTCPClient())
            {
                // open an SSL stream
                this.isConnected = this.OpenSecureStream();
                return this.isConnected;
            }

            return this.isConnected;
        }

        /// <summary>
        /// Open a TCP connection
        /// </summary>
        /// <returns></returns>
        private bool ConnectTCPClient()
        {
            int connectionAttempts = 0;
            while (connectionAttempts < this.connectionRetries && (this.apnsTCPClient == null || !this.apnsTCPClient.Connected))
            {
                connectionAttempts++;
                try
                {
                    this.apnsTCPClient = new TcpClient();
                    this.apnsTCPClient.Connect(this.Host, this.Port);
                }
                catch (SocketException ex)
                {
                    if (this.NotificationError != null)
                    {
                        this.NotificationError(this, new NotificationEventArgs(ex));
                    }

                    this.DisconnectTCPClient();
                }
            }

            if (this.apnsTCPClient == null || !this.apnsTCPClient.Connected)
            {
                if (this.NotificationError != null)
                {
                    this.NotificationError(this, new NotificationEventArgs(new NotificationException("Too many connection attempts for " + this.Host)));
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Open a SSL stream over the connection
        /// </summary>
        /// <returns></returns>
        private bool OpenSecureStream()
        {
            this.apnsSecureStream = new SslStream(this.apnsTCPClient.GetStream(), false, new RemoteCertificateValidationCallback(this.ValidateServerCertificate), new LocalCertificateSelectionCallback(this.SelectLocalCertificate));

            try
            {
                this.apnsSecureStream.AuthenticateAsClient(this.Host, this.certificates, System.Security.Authentication.SslProtocols.Ssl3, false);
            }
            catch (System.Security.Authentication.AuthenticationException ex)
            {
                if (this.NotificationError != null)
                {
                    this.NotificationError(this, new NotificationEventArgs(ex));
                }

                return false;
            }

            if (!this.apnsSecureStream.IsMutuallyAuthenticated)
            {
                if (this.NotificationError != null)
                {
                    this.NotificationError(this, new NotificationEventArgs(new NotificationException("APNS: SSL Stream Failed to authenticate for " + this.Host)));
                }

                return false;
            }

            if (!this.apnsSecureStream.CanWrite)
            {
                if (this.NotificationError != null)
                {
                    this.NotificationError(this, new NotificationEventArgs(new NotificationException("APNS: SSL Stream is not writable for " + this.Host)));
                }

                return false;
            }

            return true;
        }

        private void CloseSecureStream()
        {
            this.apnsSecureStream.Close();
            this.apnsSecureStream.Dispose();
            this.apnsSecureStream = null;
        }

        private void DisconnectTCPClient()
        {
            this.apnsTCPClient.Close();
            this.apnsTCPClient = null;
            this.isConnected = false;
        }

        // we are accepting server connections
        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        // send our certificate since this uses mutual auth
        private X509Certificate SelectLocalCertificate(
                                object sender,
                                string targetHost,
                                X509CertificateCollection localCertificates,
                                X509Certificate remoteCertificate,
                                string[] acceptableIssuers)
        {
            return this.certificate;
        }

        #endregion

        /// <summary>
        /// aps class is used for serialization, that's it
        /// </summary>
        private class ApnsMessage
        {
            public string Alert
            {
                get;
                set;
            }

            public int? Badge
            {
                get;
                set;
            }

            public string Sound
            {
                get;
                set;
            }
        }

        // this class too is used only for serialization
        private class AppleNotification
        {
            public AppleNotification()
            {
                this.Aps = new ApnsMessage();
            }

            public ApnsMessage Aps
            {
                get;
                private set;
            }
        }

        #endregion
    }
}

