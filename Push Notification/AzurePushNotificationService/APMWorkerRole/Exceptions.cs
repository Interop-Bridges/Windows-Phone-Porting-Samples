// --------------------------------------------------------------------
// <copyright file="Exceptions.cs" company="Microsoft Corp">
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
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using MsgHelperLib.Messages;
    using MsgHelperLib.Model;

    /// <summary>
    /// NotificationEventArgs class is used for the event handlers for connection class errors.
    /// </summary>
    internal class NotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the NotificationEventArgs class.
        /// </summary>
        /// <param name="exception">Exception to be passed as part of the event args.</param>
        public NotificationEventArgs(Exception exception)
        {
            this.Exc = exception;
        }

        /// <summary>
        /// Gets or sets the notification Event Args information
        /// </summary>
        public Exception Exc
        {
            get;
            set;
        }
    }

    /// <summary>
    /// The class used to send an exception for notification exception.
    /// </summary>
    [Serializable]
    internal class NotificationFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the NotificationFormatException class.
        /// </summary>
        /// <param name="message">Notification that caused the Exception.</param>
        public NotificationFormatException(PushMessage message)
            : base(string.Format(CultureInfo.InvariantCulture, "Notification payload larger than maximum allowed"))
        {
            this.NotificationMessage = message;
        }

        /// <summary>
        /// Initializes a new instance of the NotificationFormatException class.
        /// </summary>
        /// <param name="message">Message being added. </param>
        /// <param name="exception">The original or inner exception.</param>
        public NotificationFormatException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotificationFormatException class.
        /// </summary>
        public NotificationFormatException() :
            base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotificationFormatException class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public NotificationFormatException(string message) :
            base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotificationFormatException class.
        /// </summary>
        /// This is needed for ISerializable interface
        /// <param name="serializationInfo">SerializationInfo provides the class where the class is serialized.</param>
        /// <param name="streamingContext">Additional StreamingContext class.</param>
        protected NotificationFormatException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        {
            if (serializationInfo != null)
            {
                PushMessageType messageType = (PushMessageType)serializationInfo.GetInt16("MessageType");
                string subscriptionName = serializationInfo.GetString("SubscriptionName");
                if ((PushMessageType)this.NotificationMessage.MessageType == PushMessageType.Raw)
                {
                    string rawMessageText = serializationInfo.GetString("rawMessageText");
                    RawMessage rawMessage = new RawMessage(subscriptionName, rawMessageText);
                    this.NotificationMessage = rawMessage;
                }
                else if ((PushMessageType)this.NotificationMessage.MessageType == PushMessageType.Toast)
                {
                    string tileMessageText = serializationInfo.GetString("toastMessageText");
                    ToastMessage toastMessage = new ToastMessage(subscriptionName, tileMessageText);
                    this.NotificationMessage = toastMessage;
                }
                else if ((PushMessageType)this.NotificationMessage.MessageType == PushMessageType.Tile)
                {
                    string tileMessageTitle = serializationInfo.GetString("tileMessageTitle");
                    string tileMessageCount = serializationInfo.GetString("tileMessageCount");
                    string tileMessageUrl = serializationInfo.GetString("tileMessageUrl");
                    TileMessage tileMessage = new TileMessage(subscriptionName, tileMessageTitle, tileMessageCount, tileMessageUrl);
                    this.NotificationMessage = tileMessage;
                }
                else if ((PushMessageType)this.NotificationMessage.MessageType == PushMessageType.Iphone)
                {
                    string messageAlert = serializationInfo.GetString("iPhoneMessageAlert");
                    string messageBadge = serializationInfo.GetString("iPhoneMessageBadge");
                    string messageSound = serializationInfo.GetString("iPhoneMessageSound");
                    iPhoneMessage iphoneMessage = new iPhoneMessage(subscriptionName, messageAlert, messageBadge, messageSound);
                    this.NotificationMessage = iphoneMessage;
                }
                else if ((PushMessageType)this.NotificationMessage.MessageType == PushMessageType.Common)
                {
                    string messageTitle = serializationInfo.GetString("commmonMessageTitle");
                    int messageCount = serializationInfo.GetInt32("commonMessageCount");
                    string messageImage = serializationInfo.GetString("commonMessageImage");
                    string messageSound = serializationInfo.GetString("commonMessageSound");
                    CommonMessage commonMessage = new CommonMessage(subscriptionName, messageTitle, messageCount, messageImage, messageSound);
                    this.NotificationMessage = commonMessage;
                }
            }
        }

        /// <summary>
        /// Gets  the notification message that caused the Exception
        /// </summary>
        public PushMessage NotificationMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// Serializes an instance of NotificationFormatException class.
        /// </summary>
        /// <param name="info">SerializationInfo instance to deserialize the object.</param>
        /// <param name="context">Supporting the context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
            {
                info.AddValue("MessageType", this.NotificationMessage.MessageType);
                info.AddValue("SubscriptionName", this.NotificationMessage.SubscriptionName);
                if ((PushMessageType)this.NotificationMessage.MessageType == PushMessageType.Raw)
                {
                    info.AddValue("rawMessageText", (this.NotificationMessage as RawMessage).Raw);
                }
                else if ((PushMessageType)this.NotificationMessage.MessageType == PushMessageType.Toast)
                {
                    info.AddValue("toastMessageText", (this.NotificationMessage as ToastMessage).Toast);
                }
                else if ((PushMessageType)this.NotificationMessage.MessageType == PushMessageType.Tile)
                {
                    info.AddValue("tileMessageTitle", (this.NotificationMessage as TileMessage).Title);
                    info.AddValue("tileMessageCount", (this.NotificationMessage as TileMessage).Count);
                    info.AddValue("tileMessageUrl", (this.NotificationMessage as TileMessage).Image);
                }
                else if ((PushMessageType)this.NotificationMessage.MessageType == PushMessageType.Iphone)
                {
                    info.AddValue("iPhoneMessageAlert", (this.NotificationMessage as iPhoneMessage).Alert);
                    info.AddValue("iPhoneMessageBadge", (this.NotificationMessage as iPhoneMessage).Badge);
                    info.AddValue("iPhoneMessageSound", (this.NotificationMessage as iPhoneMessage).Sound);
                }
                else if ((PushMessageType)this.NotificationMessage.MessageType == PushMessageType.Common)
                {
                    info.AddValue("commmonMessageTitle", (this.NotificationMessage as CommonMessage).Title);
                    info.AddValue("commmonMessageCount", (this.NotificationMessage as CommonMessage).Count);
                    info.AddValue("commmonMessageImage", (this.NotificationMessage as CommonMessage).Image);
                    info.AddValue("commmonMessageSound", (this.NotificationMessage as CommonMessage).Sound);
                }
            }
        }
    }

    /// <summary>
    /// DeviceIdFormatException provides an exception from connection class if device Id is incorrect.
    /// </summary>
    [Serializable]
    internal class DeviceIdFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the DeviceIdFormatException class.
        /// </summary>
        /// <param name="deviceId">The device id that has the format issues.</param>
        public DeviceIdFormatException(string deviceId)
            : base(string.Format(CultureInfo.InvariantCulture, "Device ID length / format incorrect"))
        {
            this.DeviceId = deviceId;
        }

        /// <summary>
        /// Initializes a new instance of the DeviceIdFormatException class.
        /// </summary>
        /// <param name="message">Message to be included in the exception.</param>
        /// <param name="exception">Inner exception.</param>
        public DeviceIdFormatException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DeviceIdFormatException class.
        /// </summary>
        public DeviceIdFormatException() :
            base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the DeviceIdFormatException class.
        /// </summary>
        /// <param name="serializationInfo">Used to initialize a new instance by deserializing.</param>
        /// <param name="streamingContext">Stream context.</param>
        protected DeviceIdFormatException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        {
            if (serializationInfo != null)
            {
                this.DeviceId = serializationInfo.GetString("DeviceId");
            }
        }

        /// <summary>
        /// Gets the DeviceId, the information included with the exception.
        /// </summary>
        public string DeviceId
        {
            get;
            private set;
        }

        /// <summary>
        /// Used to serialize the edception instance.
        /// </summary>
        /// <param name="info">The serialization info to serialize the instance to.</param>
        /// <param name="context">Streamning context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
            {
                info.AddValue("DeviceId", this.DeviceId);
            }
        }
    }

    /// <summary>
    /// NotificationException class is used to encapsulate any notification exception.
    /// </summary>
    [Serializable]
    internal class NotificationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the NotificationException class.
        /// </summary>
        /// <param name="message">Notification that caused the Exception</param>
        public NotificationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotificationException class.
        /// </summary>
        /// <param name="message">Message to be included in the exception.</param>
        /// <param name="exception">Inner exception.</param>
        public NotificationException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotificationException class.
        /// </summary>
        public NotificationException() :
            base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotificationException class.
        /// </summary>
        /// <param name="serializationInfo">Used to initialize a new instance by deserializing.</param>
        /// <param name="streamingContext">Stream context.</param>
        protected NotificationException(SerializationInfo serializationInfo, StreamingContext streamingContext) :
            base(serializationInfo, streamingContext)
        {
            if (serializationInfo != null)
            {
                this.PushMessageText = serializationInfo.GetString("Message");
            }
        }

        /// <summary>
        /// Gets the notification message that caused the Exception.
        /// </summary>
        public string PushMessageText
        {
            get;
            private set;
        }

        /// <summary>
        /// Used to serialize the exception instance.
        /// </summary>
        /// <param name="info">The serialization info to serialize the instance to.</param>
        /// <param name="context">Streamning context.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
            {
                info.AddValue("Message", this.PushMessageText);
            }
        }
    }
}
