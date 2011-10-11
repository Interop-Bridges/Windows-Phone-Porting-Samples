// --------------------------------------------------------------------
// <copyright file="Device.cs" company="Microsoft Corp">
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
    /// Models the device in the azure storage
    /// optimization here is to use deviceID as the rowkey and type as the partition key. 
    /// </summary>
    public class DeviceDataModel : TableServiceEntity
    {
        /// <summary>
        /// Initializes a new instance of the DeviceDataModel class.
        /// </summary>
        /// <param name="deviceType">The type of the device being added.</param>
        /// <param name="deviceId">The device id of the device being added</param>
        public DeviceDataModel(string deviceType, string deviceId)
            : base()
        {
             PartitionKey = Guid.NewGuid().ToString();
             RowKey = String.Empty;
             this.DeviceType = deviceType;
             this.DeviceId = deviceId;
        }

        /// <summary>
        /// Initializes a new instance of the DeviceDataModel class.
        /// </summary>
        public DeviceDataModel()
            : base()
        {
            PartitionKey = Guid.NewGuid().ToString();
            RowKey = String.Empty;
        }

        /// <summary>
        /// Gets or sets the type of the device being added. 
        /// </summary>
        public string DeviceType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Device id of the device added. 
        /// This information is used for all types of the devices. 
        /// </summary>
        public string DeviceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the address / URI of the device added. 
        /// This is used only for the WP7 devices. We store a blank strinbg for others
        /// </summary>
        public string Address
        {
            get;
            set;
        }
    }
}
