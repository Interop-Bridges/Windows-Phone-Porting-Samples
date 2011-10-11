// --------------------------------------------------------------------
// <copyright file="Queue.cs" company="Microsoft Corp">
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
namespace MsgHelperLib.Queue
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Web.Script.Serialization;
    using System.Xml;
    using System.Xml.Serialization;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;
    using MsgHelperLib.Messages;

    /// <summary>
    /// provides abstraction for azure queue
    /// </summary>
    public class PushMessageQueue
    {
        private CloudQueue queue;

        /// <summary>
        /// Initializes a new instance of the PushMessageQueue class.
        /// </summary>
        /// Creates Azure queue.
        public PushMessageQueue()
        {
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));

            // retrieve a reference to the messages queue 
            var queueClient = storageAccount.CreateCloudQueueClient();
            this.queue = queueClient.GetQueueReference("messagequeue");
            this.queue.CreateIfNotExist();
        }

        /// <summary>
        /// enque a message by taking our PushMessage, serializing it and adding its type to the front of the message
        /// </summary>
        /// <param name="msg"></param>
        public void Enque(PushMessage msg)
        {
            string msgStr = ToString(msg);
            var queueMsg = new CloudQueueMessage(msgStr);
            this.queue.AddMessage(queueMsg);
        }

        /// <summary>
        /// dequeue the message, read its type and then deserialize it appropriately
        /// Once the message is deserialized, it is deleted so that another worker does not pick it up
        /// worker will call this method to dequeue the message. If it does not exist, it will sleep for a while
        /// </summary>
        /// <returns>Returns a PushMessage dequeued from the queue. Otherwise null.</returns>
        public PushMessage Deque()
        {
            if (this.queue.Exists())
            {
                PushMessage pm = null;
                var msg = this.queue.GetMessage();
                if (msg != null)
                {
                    string msgStr = msg.AsString;
                    pm = FromString(msgStr, typeof(PushMessage)) as PushMessage;
                    this.queue.DeleteMessage(msg);
                    return pm;
                }
            }

            return null;
        }

        private static object FromString(string xmlStr, System.Type objType)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            return jser.Deserialize(xmlStr, objType);
        }

        private static string ToString(PushMessage o)
        {
            JavaScriptSerializer jser = new JavaScriptSerializer();
            string contents = jser.Serialize(o);
            return contents;
        }
    }
}
