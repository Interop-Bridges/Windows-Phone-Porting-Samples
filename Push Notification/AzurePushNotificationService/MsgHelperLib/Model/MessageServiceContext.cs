// --------------------------------------------------------------------
// <copyright file="MessageServiceContext.cs" company="Microsoft Corp">
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
    using Microsoft.WindowsAzure.StorageClient;

    /// <summary>
    /// MessageServiceContext manages the Azure tables. 
    /// </summary>
    public class MessageServiceContext : TableServiceContext
    {
        /// <summary>
        /// SubscriptionsTable saves the subscriptions.
        /// </summary>
        public const string SubscriptionsTableName = "SubscriptionsTable";

        /// <summary>
        /// DeviceTable saves the device information.
        /// </summary>
        public const string DeviceTableName = "DeviceTable";

        /// <summary>
        /// Subscriptions info table saves the key information for the subscriptions such as name and descriptions.
        /// </summary>
        public const string SubscriptionsInfoTableName = "SubscriptionInfoTable";

        /// <summary>
        /// Initializes a new instance of the MessageServiceContext class.
        /// </summary>
        /// <param name="baseAddress">Base address of the service.</param>
        /// <param name="credentials">Credentials to use to connect to the context.</param>
        internal MessageServiceContext(string baseAddress, StorageCredentials credentials)
            : base(baseAddress, credentials)
        {
            this.IgnoreResourceNotFoundException = true;
        }

        /// <summary>
        /// Gets the device - subscriptions mapping table. 
        /// Used most frequently for each message.
        /// </summary>
        public IQueryable<SubscriptionDataModel> SubscriptionsTable
        {
            get
            {
                return this.CreateQuery<SubscriptionDataModel>(SubscriptionsTableName);
            }
        }

        /// <summary>
        /// Gets a Subscriptions Info table - used to keep name and descriptions.
        /// </summary>
        public IQueryable<SubscriptionInfoDataModel> SubscriptionsInfoTable
        {
            get
            {
                return this.CreateQuery<SubscriptionInfoDataModel>(SubscriptionsInfoTableName);
            }
        }

        /// <summary>
        /// Gets the table that keep the information about all devices in the system.
        /// </summary>
        public IQueryable<DeviceDataModel> DeviceTable
        {
            get
            {
                return this.CreateQuery<DeviceDataModel>(DeviceTableName);
            }
        }
    }
}
