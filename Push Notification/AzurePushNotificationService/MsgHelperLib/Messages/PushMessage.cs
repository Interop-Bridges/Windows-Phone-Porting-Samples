// --------------------------------------------------------------------
// <copyright file="PushMessage.cs" company="Microsoft Corp">
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
namespace MsgHelperLib.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// PushMessageTyps provides various types of messages being supported.
    /// </summary>
    public enum PushMessageType 
    { 
        /// <summary>
        /// This is not used.
        /// </summary>
        None = 0, 

        /// <summary>
        /// Toast type used only for WP7
        /// </summary>
        Toast = 1, 

        /// <summary>
        /// Raw message type used for WP7
        /// </summary>
        Raw = 2, 

        /// <summary>
        /// Tile message type for WP7
        /// </summary>
        Tile = 3, 

        /// <summary>
        /// iPhone message typs for iOS devices
        /// </summary>
        Iphone = 4, 

        /// <summary>
        /// Common message type can be used to send to iPhone and WP7 devices.
        /// </summary>
        Common = 5 
    }

    /// <summary>
    /// parent class for all message types. Since message body is kept as a collection of key/value pairs, it is easy to add new types
    /// the messages of this type are sent in the Azure queue from the web role to worker role
    /// </summary>
    public class PushMessage
    {
        /// <summary>
        /// Initializes a new instance of the PushMessage class.
        /// </summary>
        /// <param name="subscriptionName">The name of the subscription.</param>
        /// <param name="type">The type of the message.</param>
        public PushMessage(string subscriptionName, short type)
        {
            this.SubscriptionName = subscriptionName;
            this.MessageType = type;
            this.Message = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the PushMessage class.
        /// </summary>
        public PushMessage()
        {
        }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        public short MessageType 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the name of the subscription to send the message to.
        /// </summary>
        public string SubscriptionName 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets message which is a dictionary of various message components.
        /// </summary>
        public Dictionary<string, string> Message 
        { 
            get; 
            set; 
        }
    }
}
