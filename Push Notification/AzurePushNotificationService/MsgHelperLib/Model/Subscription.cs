// --------------------------------------------------------------------
// <copyright file="Subscription.cs" company="Microsoft Corp">
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
namespace MsgHelperLib.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;

    /// <summary>
    /// models subscription for each device
    /// again the optimization can use different key
    /// </summary>
    public class SubscriptionDataModel : TableServiceEntity
    {
        /// <summary>
        /// Initializes a new instance of the SubscriptionDataModel class.
        /// </summary>
        /// <param name="subscription">Name of the subscription to subscribe to.</param>
        /// <param name="deviceId">The id of the device subscribing.</param>
        public SubscriptionDataModel(string subscription, string deviceId)
            : base(subscription, deviceId)
        {
            PartitionKey = Guid.NewGuid().ToString();
            RowKey = String.Empty;
            this.Subscription = subscription;
            this.DeviceId = deviceId;
        }

        /// <summary>
        /// Initializes a new instance of the SubscriptionDataModel class.
        /// </summary>
        public SubscriptionDataModel()
            : base()
        {
            PartitionKey = Guid.NewGuid().ToString();
            RowKey = String.Empty;
        }

        /// <summary>
        /// Gets or sets of sets the device id of the subscription.
        /// </summary>
        public string DeviceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the subscription name.
        /// </summary>
        /// provides a subscription string such as football, baseball, Oakland A's and what have you
        public string Subscription
        {
            get;
            set;
        }
    }
}
