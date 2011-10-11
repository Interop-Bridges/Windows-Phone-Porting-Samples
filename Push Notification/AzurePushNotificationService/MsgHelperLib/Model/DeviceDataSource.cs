// --------------------------------------------------------------------
// <copyright file="DeviceDataSource.cs" company="Microsoft Corp">
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
    /// Handles storage in azure storage. All methods use LINQ for queries. Each method is rather straight forward LINQ statement
    /// </summary>
    public class DeviceDataSource
    {
        private MessageServiceContext serviceContext = null;

        /// <summary>
        /// Initializes a new instance of the DeviceDataSource class.
        /// </summary>
        public DeviceDataSource()
        {
            // Get the settings from the Service Configuration file
            // Var account = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            var account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));

            // Create data table from MessageServiceContext 
            // It is recommended the data tables should be only created once. It is typically done as a  
            // provisioning step and rarely in application code. 
            this.serviceContext = new MessageServiceContext(account.TableEndpoint.ToString(), account.Credentials);
            account.CreateCloudTableClient().CreateTableIfNotExist(MessageServiceContext.DeviceTableName);
        }

        /// <summary>
        /// Select method to simplify searching.
        /// </summary>
        /// <param name="deviceId">The device id of the device being searched.</param>
        /// <param name="type">The type of the device being searched.</param>
        /// <returns>Returns the DeviceDataModel if found otherwise returns null.</returns>
        public DeviceDataModel SelectByDeviceIdAndType(string deviceId, string type)
        {
            try
            {
                DeviceDataModel device = this.serviceContext.DeviceTable.Where(s => s.DeviceId == deviceId && s.DeviceType == type).FirstOrDefault();
                return device;
            }
            catch (DataServiceQueryException)
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Select device by DeviceID.
        /// </summary>
        /// <param name="deviceId">The device id being searched.</param>
        /// <returns>Returns the DeviceDataModel if found otherwise returns null.</returns>
        public DeviceDataModel SelectByDeviceId(string deviceId)
        {
            try
            {
                DeviceDataModel device = this.serviceContext.DeviceTable.Where(s => s.DeviceId == deviceId).First();
                return device;
            }
            catch (DataServiceQueryException)
            {
                return null;
            }
            catch (ArgumentNullException)
            {
                return null;
            }

            //catch (Exception)
            //{
            //    return null;
            //}
        }

        /// <summary>
        /// Returns all devices of a particular type. This is an administrative call. Not used currently
        /// </summary>
        /// <param name="type">The type of the device to search.</param>
        /// <returns>Returns the collection of DeviceDataModel if found.</returns>
        public IEnumerable<DeviceDataModel> SelectByType(string type)
        {
            IEnumerable<DeviceDataModel> devicesByType = this.serviceContext.DeviceTable.Where(s => s.DeviceType == type);
            return devicesByType;
        }

        /// <summary>
        /// Add a device to the table
        /// </summary>
        /// <param name="newItem">The DeviceDataModel item to be added.</param>
        public void Insert(DeviceDataModel newItem)
        {
            this.serviceContext.AddObject(MessageServiceContext.DeviceTableName, newItem);
            this.serviceContext.SaveChanges();
        }

        /// <summary>
        /// Remove a device from the table
        /// </summary>
        /// <param name="item">The DeviceDataModel item to be deleted.</param>
        public void Delete(DeviceDataModel item)
        {
            this.serviceContext.DeleteObject(item);
            this.serviceContext.SaveChanges();
        }

        /// <summary>
        /// Update a device. For WP7 device, URI will get updated often. 
        /// </summary>
        /// <param name="updatedItem">The item to be updated.</param>
        public void Update(DeviceDataModel updatedItem)
        {
            // We only need to update WP7 devices.
            this.serviceContext.UpdateObject(updatedItem);
            this.serviceContext.SaveChanges();
        }
    }
}
