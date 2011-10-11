// --------------------------------------------------------------------
// <copyright file="RawMessage.cs" company="Microsoft Corp">
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
    /// Helper class to create a raw message
    /// Uses a property Message to return the component of the raw notification
    /// </summary>
    public class RawMessage : PushMessage
    {
        /// <summary>
        /// Initializes a new instance of the RawMessage class.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription to send the message to.</param>
        /// <param name="rawMessage">The text of the raw message to be sent.</param>
        public RawMessage(string subscriptionName, string rawMessage)
            : base(subscriptionName, (short)PushMessageType.Raw)
        {
            Message.Add("raw", rawMessage);
        }

        /// <summary>
        /// Initializes a new instance of the RawMessage class.
        /// </summary>
        public RawMessage()
            : base()
        {
        }

        /// <summary>
        /// Gets the message text.
        /// </summary>
        public string Raw
        {
            get
            {
                return Message["raw"];
            }
        }
    }
}
