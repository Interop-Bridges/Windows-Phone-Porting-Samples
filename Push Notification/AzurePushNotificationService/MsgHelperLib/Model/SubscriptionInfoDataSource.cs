// --------------------------------------------------------------------
// <copyright file="SubscriptionInfoDataSource.cs" company="Microsoft Corp">
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
    using System.Data.Services.Client;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;

    /// <summary>
    /// Manages the Azure tables that stores Subscription info.
    /// </summary>
    public class SubscriptionInfoDataSource
    {
        private MessageServiceContext serviceContext = null;

        /// <summary>
        /// Initializes a new instance of the SubscriptionInfoDataSource class.
        /// </summary>
        public SubscriptionInfoDataSource()
        {
            // Get the settings from the Service Configuration file
            var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));

            // Create data table from MessageServiceContext 
            // It is recommended the data tables should be only created once. It is typically done as a  
            // provisioning step and rarely in application code. 
            this.serviceContext = new MessageServiceContext(account.TableEndpoint.ToString(), account.Credentials);
            account.CreateCloudTableClient().CreateTableIfNotExist(MessageServiceContext.SubscriptionsInfoTableName);
        }

        /// <summary>
        /// Returns a list of Subscriptions (name, descriptions) in the system. 
        /// </summary>
        /// <returns>returns the list of subscription info records.</returns>
        public IEnumerable<SubscriptionInfoDataModel> Subscriptions()
        {
            return this.serviceContext.SubscriptionsInfoTable;
        }

        /// <summary>
        /// Provide details about a particular subscription i.e. description etc. 
        /// </summary>
        /// <param name="subscriptionName">Name of the subscription to search.</param>
        /// <returns>Returns the subscripton details (name, description).</returns>
        public SubscriptionInfoDataModel SelectBySubscription(string subscriptionName)
        {
            try
            {
                SubscriptionInfoDataModel sidm = this.serviceContext.SubscriptionsInfoTable.Where(s => (s.SubscriptionName == subscriptionName)).FirstOrDefault();
                return sidm;
            }
            catch (DataServiceQueryException)
            {
                return null;
            }
        }

        // CRUD operations below

        /// <summary>
        /// Used to create / insert a new subscription.
        /// </summary>
        /// <param name="newItem">Subscription info record to be inserted.</param>
        public void Insert(SubscriptionInfoDataModel newItem)
        {
            this.serviceContext.AddObject(MessageServiceContext.SubscriptionsInfoTableName, newItem);
            this.serviceContext.SaveChanges();
        }

        /// <summary>
        /// Delete a subscription from the system. Higher level should clean up subscriptions in the system, i.e. remove devices from the subscription
        /// </summary>
        /// <param name="item">Subscription info record to be deleted.</param>
        public void Delete(SubscriptionInfoDataModel item)
        {
            this.serviceContext.DeleteObject(item);
            this.serviceContext.SaveChanges();
        }

        /// <summary>
        /// Subscription info record to be deleted.
        /// </summary>
        /// <param name="updatedItem">Subscription info record to be updated.</param>
        public void Update(SubscriptionInfoDataModel updatedItem)
        {
            throw new NotImplementedException();
        }
    }
}
