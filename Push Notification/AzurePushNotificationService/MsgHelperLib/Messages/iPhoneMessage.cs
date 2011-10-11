// --------------------------------------------------------------------
// <copyright file="iPhoneMessage.cs" company="Microsoft Corp">
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
    using System.Web.Script.Serialization;

    /// <summary>
    /// Helper class for returning an iPhoneMessage
    /// </summary>
    public class iPhoneMessage : PushMessage
    {
        /// <summary>
        /// Initializes a new instance of the iPhoneMessage class. 
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription for which message is sent.</param>
        /// <param name="alert">Alert / text of the message.</param>
        /// <param name="badge">Badge that will be shown application icon.</param>
        /// <param name="sound">Sound file name being sent.</param>
        public iPhoneMessage(string subscriptionName, string alert, string badge, string sound)
            : base(subscriptionName, (short)PushMessageType.Iphone)
        {
            Message.Add("title", alert);
            Message.Add("count", badge);
            Message.Add("sound", sound);
        }

        /// <summary>
        /// Initializes a new instance of the iPhoneMessage class. 
        /// </summary>
        public iPhoneMessage()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets the title as a Alert property.
        /// </summary>
        public string Alert
        {
            get
            {
                return Message["title"];
            }

            set
            {
                Message["title"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the badge of the message. 
        /// </summary>
        public string Badge
        {
            get
            {
                return Message["count"];
            }

            set
            {
                Message["count"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the sound file. 
        /// </summary>
        public string Sound
        {
            get
            {
                return Message["sound"];
            }

            set
            {
                Message["sound"] = value;
            }
        }
    }
}
