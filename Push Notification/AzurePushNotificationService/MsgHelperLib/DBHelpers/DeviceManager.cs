// --------------------------------------------------------------------
// <copyright file="DeviceManager.cs" company="Microsoft Corp">
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
namespace MsgHelperLib.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MsgHelperLib.Common;
    using MsgHelperLib.Model;

    /// <summary>
    /// Class manages all devices in the system and persists them.
    /// </summary>
    public class DeviceManager
    {
        private const int IphoneDeviceIdLength = 4;
        private DeviceDataSource dds;

        /// <summary>
        /// Initializes a new instance of the DeviceManager class.
        /// </summary>
        public DeviceManager()
        {
            this.dds = new DeviceDataSource();
        }

        /// <summary>
        /// Delete device from the d/b
        /// </summary>
        /// <param name="deviceId">The device id of the device delete.</param>
        /// <returns>Returns either success or error.</returns>
        public PushMessageError DeleteDevice(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return PushMessageError.ErrorIllegalDeviceId;

            DeviceDataModel ddm = this.dds.SelectByDeviceId(deviceId);
            if (ddm == null)
            {
                return PushMessageError.ErrorDeviceNotFound;
            }

            this.dds.Delete(ddm);
            return PushMessageError.Success;    
        }

        /// <summary>
        /// Adds the device or updates it. For WP7, the URI will be updated frequently. 
        /// deviceID is maintained as the index into the d/b
        /// </summary>
        /// <param name="devTypeName">The type of the device being added or updated.</param>
        /// <param name="deviceId">The device id of the device being added or updated.</param>
        /// <param name="deviceUri">The device uri. Used only for WP7 and empty string for others.</param>
        /// <returns>Returns success or an error.</returns>
        public PushMessageError AddOrUpdateDevice(string devTypeName, string deviceId, string deviceUri)
        {
            DeviceDataModel ddm = this.dds.SelectByDeviceIdAndType(deviceId, devTypeName);
            if (ddm == null)
            {
                // No such device, create it
                ddm = new DeviceDataModel(devTypeName, deviceId);
                ddm.Address = deviceUri;
                this.dds.Insert(ddm);
                return PushMessageError.Success;
            }
            else
            {
                // For WP7, we will update the URI with the new value
                ddm.Address = deviceUri;
                this.dds.Update(ddm);
                return PushMessageError.Success;
            }
        }

        /// <summary>
        /// Is the device in the system. If it is, return true. Otherwise false
        /// </summary>
        /// <param name="deviceId">The device id of the device being checked.</param>
        /// <returns>Returns true if the device already registered. </returns>
        public bool IsDeviceRegistered(string deviceId)
        {
            DeviceDataModel ddm = this.dds.SelectByDeviceId(deviceId);
            if (ddm == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal DeviceDataModel GetDevice(string deviceId)
        {
            DeviceDataModel ddm = this.dds.SelectByDeviceId(deviceId);
            return ddm;
        }
    }
}
