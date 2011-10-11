// --------------------------------------------------------------------
// <copyright file="TileMessage.cs" company="Microsoft Corp">
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
    /// helper class to create a toast message
    /// Uses three different properites to return components of the toast notification
    /// </summary>
    public class TileMessage : PushMessage
    {
        /// <summary>
        /// Initializes a new instance of the TileMessage class.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription to send the message.</param>
        /// <param name="title">Title of the message.</param>
        /// <param name="count">Count - the number to be shown on the WP7 tile.</param>
        /// <param name="url">URL of the image to be shown on the tile</param>
        public TileMessage(string subscriptionName, string title, string count, string url)
            : base(subscriptionName, (short)PushMessageType.Tile)
        {
            Message.Add("title", title);
            Message.Add("count", count);
            Message.Add("url", url);
        }

        /// <summary>
        /// Initializes a new instance of the TileMessage class.
        /// </summary>
        public TileMessage()
            : base()
        {
        }

        /// <summary>
        /// Gets the title of the message.
        /// </summary>
        public string Title
        {
            get
            {
                return Message["title"];
            }
        }

        /// <summary>
        /// Gets the number to be shown on the WP7 tile.
        /// </summary>
        public int Count
        {
            get
            {
                return int.Parse(Message["count"], CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the image to be shonw on the tile.
        /// </summary>
        public string Image
        {
            get
            {
                return Message["url"];
            }
        }
    }
}
