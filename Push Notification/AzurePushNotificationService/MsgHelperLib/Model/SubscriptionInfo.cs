// --------------------------------------------------------------------
// <copyright file="SubscriptionInfo.cs" company="Microsoft Corp">
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
    /// Manages subscripion Info. Just has name and description.
    /// Can use sub name as the row key as optimization.
    /// </summary>
    public class SubscriptionInfoDataModel : TableServiceEntity
    {
        /// <summary>
        /// Initializes a new instance of the SubscriptionInfoDataModel class.
        /// </summary>
        public SubscriptionInfoDataModel()
            : base()
        {
            PartitionKey = Guid.NewGuid().ToString();
            RowKey = String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the SubscriptionInfoDataModel class.
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription being created.</param>
        /// <param name="description">Description of the subscription being added.</param>
        public SubscriptionInfoDataModel(string subscriptionName, string description)
            : base()
        {
            PartitionKey = Guid.NewGuid().ToString();
            RowKey = String.Empty;
            this.SubscriptionName = subscriptionName;
            this.Description = description;
        }

        /// <summary>
        /// Gets or sets name of the subscription being created.
        /// </summary>
        public string SubscriptionName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets description of the subscription being created.
        /// </summary>
        public string Description
        {
            get;
            set;
        }
    }
}
