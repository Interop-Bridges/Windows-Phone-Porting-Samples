// --------------------------------------------------------------------
// <copyright file="Connection.cs" company="Microsoft Corp">
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
    /// Abstract class for various notification service connection classes.
    /// </summary>
    internal abstract class Connection
    {
        #region Delegates and Events

        /// <summary>
        /// Occurs when a General Error is thrown
        /// </summary>
        public virtual event EventHandler<NotificationEventArgs> NotificationError;

        /// <summary>
        /// Occurs when a Notification is not per specifications
        /// </summary>
        public virtual event EventHandler<NotificationEventArgs> NotificationFormatError;

        /// <summary>
        /// Occurs when a Device ID has incorrect format
        /// </summary>
        public virtual event EventHandler<NotificationEventArgs> DeviceIdFormatError;

        /// <summary>
        /// Occurs when a notification fails to deliver
        /// </summary>
        public virtual event EventHandler<NotificationEventArgs> NotificationFailed;

        #endregion

        /// <summary>
        /// Gets the device type supported by the connection class.
        /// </summary>
        public abstract string SupportedDeviceType
        {
            get;
        }

        /// <summary>
        /// Enque and send the message to the appropriate notification service
        /// </summary>
        /// <param name="device">Device to send the message to</param>
        /// <param name="msg">The push message to send to the device</param>
        public virtual void EnqueMessage(DeviceDataModel device, PushMessage msg)
        {
            return;
        }

        /// <summary>
        /// Cleanup the state
        /// </summary>
        public virtual void Close()
        {
            return;
        }

        /// <summary>
        /// Enques and sends the message to a collection of devices
        /// </summary>
        /// <param name="devices">List of devices to send the message to</param>
        /// <param name="msg">The push message to send the message to the devices</param>
        public virtual void EnqueMessage(IEnumerable<DeviceDataModel> devices, PushMessage msg)
        {
            // Check if the message is supported is by the connection class.
            if (!this.HandlesMessageType((PushMessageType)msg.MessageType))
            {
                return;
            }

            if (devices == null)
            {
                return;
            }

            foreach (DeviceDataModel ddm in devices)
            {
                this.EnqueMessage(ddm, msg);
            }
        }

        /// <summary>
        /// This method returns if the connection class supports the message type.
        /// </summary>
        /// <param name="messageType">Check if the specific message type is supported.</param>
        /// <returns>Returns true of this message type is supported. False otherwise.</returns>
        public abstract bool HandlesMessageType(PushMessageType messageType);

        /// <summary>
        /// This method returns whether the connection class supports various message types.
        /// </summary>
        /// <param name="messageTypes">Check if the specified list of message types are supported.</param>
        /// <returns>returns a dictionary with message types and whether that message type is supported.</returns>
        public virtual Dictionary<PushMessageType, bool> HandlesMessageTypes(Array messageTypes)
        {
            Dictionary<PushMessageType, bool> handlesMessageTypes = new Dictionary<PushMessageType, bool>();
            foreach (PushMessageType messageType in messageTypes)
            {
                handlesMessageTypes.Add(messageType, this.HandlesMessageType(messageType));
            }

            return handlesMessageTypes;
        }
    }
}
