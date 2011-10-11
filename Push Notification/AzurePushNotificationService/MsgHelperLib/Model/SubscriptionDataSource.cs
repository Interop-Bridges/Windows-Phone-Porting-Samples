// --------------------------------------------------------------------
// <copyright file="SubscriptionDataSource.cs" company="Microsoft Corp">
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
    /// Manages the interface with d/b for subscription table
    /// </summary>
    public class SubscriptionDataSource
    {
        private MessageServiceContext serviceContext = null;

        /// <summary>
        /// Initializes a new instance of the SubscriptionDataSource class.
        /// </summary>
        public SubscriptionDataSource()
        {
            // Get the settings from the Service Configuration file
            var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));

            // Create data table from MessageServiceContext 
            // It is recommended the data tables should be only created once. It is typically done as a  
            // provisioning step and rarely in application code. 
            this.serviceContext = new MessageServiceContext(account.TableEndpoint.ToString(), account.Credentials);
            account.CreateCloudTableClient().CreateTableIfNotExist(MessageServiceContext.SubscriptionsTableName);
        }

        /// <summary>
        /// Update Just for completeness.
        /// </summary>
        /// This is not implemented.
        /// <param name="updatedItem">The subscription record to be deleted.</param>
        public static void Update(SubscriptionDataModel updatedItem)
        {
            // we do not update subscriptions. You create or delete
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns all subscriptions (i.e. devices) for a particular subscription.
        /// </summary>
        /// <param name="subscription">Name of the subscription.</param>
        /// <returns>Returns the detailed information of the subscription being searched.</returns>
        public IEnumerable<SubscriptionDataModel> SelectBySubscription(string subscription)
        {
            try
            {
                IEnumerable<SubscriptionDataModel> subsByType = this.serviceContext.SubscriptionsTable.Where(s => s.Subscription == subscription);
                return subsByType;
            }
            catch (DataServiceQueryException)
            {
                return null;
            }
        }

        /// <summary>
        /// Select all subscriptions for a device. A device may use this to show the user what subscriptions he/she is subscribed to.
        /// </summary>
        /// <param name="deviceId">The device id of the device being searched.</param>
        /// <returns>Returns all subscriptions the device has signed up for. </returns>
        public IEnumerable<SubscriptionDataModel> SelectByDevice(string deviceId)
        {
            try
            {
                IEnumerable<SubscriptionDataModel> subsByDevice = this.serviceContext.SubscriptionsTable.Where(s => s.DeviceId == deviceId);
                return subsByDevice;
            }
            catch (DataServiceQueryException)
            {
                return null;
            }
        }

        /// <summary>
        /// Get the details of a subscription for a device.
        /// </summary>
        /// <param name="deviceId">The device id of the device being searched.</param>
        /// <param name="subscription">The subscription for which we want details.</param>
        /// <returns>Returns the details of the device subscription.</returns>
        public SubscriptionDataModel SelectByDeviceIdAndSubscription(string deviceId, string subscription)
        {
            try
            {
                SubscriptionDataModel sdm = this.serviceContext.SubscriptionsTable.Where(s => (s.Subscription == subscription && s.DeviceId == deviceId)).FirstOrDefault();
                return sdm;
            }
            catch (DataServiceQueryException)
            {
                return null;
            }
        }

        /// <summary>
        /// This call is currently used to send list of devices that are of a particular type and signed up for a subscription.
        /// </summary>
        /// <param name="subscription">The subscription for which we want details.</param>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns>Returns a list of devices of a particular type for a subscription.</returns>
        public IEnumerable<DeviceDataModel> SelectByDeviceTypeAndSubscription(string subscription, string deviceType)
        {
            try
            {
                List<DeviceDataModel> ddmList = new List<DeviceDataModel>();
                IEnumerable<SubscriptionDataModel> sdmList = this.serviceContext.SubscriptionsTable.Where(s => (s.Subscription == subscription));
                foreach (SubscriptionDataModel sdm in sdmList) 
                {
                    ddmList.AddRange(this.serviceContext.DeviceTable.Where(d => d.DeviceId == sdm.DeviceId && d.DeviceType == deviceType));                    
                }

                return ddmList;
            }
            catch (DataServiceQueryException)
            {
                return null;
            }
        }

        /// <summary>
        /// Add a subscription to the table. Used when a device subscribes to a subscription
        /// </summary>
        /// <param name="newItem">The new subscription record being added.</param>
        public void Insert(SubscriptionDataModel newItem)
        {
            this.serviceContext.AddObject(MessageServiceContext.SubscriptionsTableName, newItem);
            this.serviceContext.SaveChanges();
        } 

        /// <summary>
        /// Used to unsubscribe from a subscription.
        /// </summary>
        /// <param name="item">The subscription record to be deleted.</param>
        public void Delete(SubscriptionDataModel item)
        {
            this.serviceContext.DeleteObject(item);
            this.serviceContext.SaveChanges();
        }

        /// <summary>
        /// Delete the device record.
        /// </summary>
        /// Deletes subscription records before deleting device details.
        /// <param name="ddm">The device to be delete.</param>
        public void DeleteDeviceSubscriptions(DeviceDataModel ddm)
        {
            IEnumerable<SubscriptionDataModel> sdmList = this.serviceContext.SubscriptionsTable.Where(s => (s.DeviceId == ddm.DeviceId));
            foreach (SubscriptionDataModel sdm in sdmList)
            {
                this.serviceContext.DeleteObject(sdm);
            }

            IEnumerable<DeviceDataModel> ddmList = this.serviceContext.DeviceTable.Where(d => d.DeviceId == ddm.DeviceId);
            foreach (DeviceDataModel tddm in ddmList)
            {
                this.serviceContext.DeleteObject(tddm);
            }

            this.serviceContext.SaveChanges();
        }
    }
}
