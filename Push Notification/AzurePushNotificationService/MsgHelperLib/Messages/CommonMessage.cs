// --------------------------------------------------------------------
// <copyright file="CommonMessage.cs" company="Microsoft Corp">
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
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// A helper class for creating commonmessage that is sent to iPhone and WP7
    /// </summary>
    public class CommonMessage : PushMessage
    {
        /// <summary>
        /// Initializes a new instance of the CommonMessage class.
        /// </summary>
        /// <param name="subscriptionName">Subscription to send message to.</param>
        /// <param name="title">Title of the message/tile/prompt.</param>
        /// <param name="count">Count on the tile or the badge.</param>
        /// <param name="image">Image on the application tile.</param>
        /// <param name="sound">Sound file name for the prompt.</param>
        public CommonMessage(string subscriptionName, string title, int count, string image, string sound)
            : base(subscriptionName, (short)PushMessageType.Common)
        {
            Message.Add("title", title);
            Message.Add("count", count.ToString(CultureInfo.InvariantCulture));
            Message.Add("url", image);
            Message.Add("sound", sound);
        }

        /// <summary>
        /// Initializes a new instance of the CommonMessage class.
        /// </summary>
        public CommonMessage()
            : base()
        {
        }

        ////These routines make it easier to access parts of the message. Message is saved as a key/value pair.         
        //// Title is returned response to both Alert and Title both, one used by iOS and another by WP7.

        /// <summary>
        /// Gets the alert which provides the title/alert for the iPhone prompt.
        /// </summary>
        public string Alert
        {
            get
            {
                return Message["title"];
            }
        }

        /// <summary>
        /// Gets the title for tile
        /// </summary>
        public string Title
        {
            get
            {
                return Message["title"];
            }
        }

        /// <summary>
        /// Gets the badge used on iPhone.
        /// </summary>
        /// same count is used as Badge and Count, one used by iOS and another by WP7.
        public short Badge
        {
            get
            {
                return short.Parse(Message["count"], CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the cound used on WP7.
        /// </summary>
        public short Count
        {
            get
            {
                return short.Parse(Message["count"], CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the sound for iPhone prompt.
        /// </summary>
        public string Sound
        {
            get
            {
                return Message["sound"];
            }
        }

        /// <summary>
        /// Gets the image used for the WP7 application tile.
        /// </summary>
        /// WP7 uses an image in a tile message.
        public string Image
        {
            get
            {
                return Message["url"];
            }
        }
    }
}
