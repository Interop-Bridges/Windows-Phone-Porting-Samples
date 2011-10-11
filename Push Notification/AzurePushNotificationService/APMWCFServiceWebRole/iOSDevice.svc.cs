// --------------------------------------------------------------------
// <copyright file="IosDevice.svc.cs" company="Microsoft Corp">
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

namespace ApmWcfServiceWebRole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;
    using Microsoft.ServiceModel.Web;
    using MsgHelperLib.Common;
    using MsgHelperLib.Helpers;
    using MsgHelperLib.Messages;
    using MsgHelperLib.Model;
    using MsgHelperLib.Queue;

    /// <summary>
    /// Service that represents an end point for iOS device registration and deregistration.
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true,
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Single)]
    public class iOSDevice : IIOSService
    {
        private const int DeviceTokenBinarySize = 32;
        private const int DeviceTokenStringSize = 64;
        private const string DeviceTypeName = "iOS"; 

        private Push push;

        /// <summary>
        /// Initializes a new instance of the IosDevice class.
        /// </summary>
        /// It also initializes supporting manager classes. 
        public iOSDevice()
        {
            this.push = new Push();
        }

        /// <summary>
        /// Registers an iOS device in the service.
        /// </summary>
        /// <param name="deviceId">The device id of the device being registered.</param>
        /// <returns>Returns "success" or an error.</returns>
        public string RegisterDevice(string deviceId)
        {
            if (IsDeviceIdValid(deviceId))
            {
                return this.push.RegisterDevice(deviceId, DeviceTypeName, String.Empty);
            }
            else
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorIllegalDeviceId), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Unregisters an iOS device.
        /// </summary>
        /// <param name="deviceId">The device id of the device being unregistered.</param>
        /// <returns>Returns "success" or am error.</returns>
        public string UnregisterDevice(string deviceId)
        {
            // Currently, this service is only allowed for admins. Devices won't unregister themselves. They will remove subscriptions. 
            if (!AuthManager.AuthenticateUser())
            {
                AuthManager.ConstructAuthResponse();
                return null;
            }

            return this.push.UnregisterDevice(deviceId);
        }

        /// <summary>
        /// IOS deviceId has a fixed format. Checking the length of the deviceid is correct.
        /// </summary>
        /// <param name="deviceId">The device id of the device being checked.</param>
        /// <returns>Returns if the device id is valid.</returns>
        private static bool IsDeviceIdValid(string deviceId)
        {
            byte[] deviceToken = new byte[deviceId.Length / 2];
            for (int i = 0; i < deviceToken.Length; i++)
            {
                bool success = byte.TryParse(deviceId.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out deviceToken[i]);
                if (!success)
                {
                    return false;
                }
            }

            if (deviceToken.Length != DeviceTokenBinarySize)
            {
                return false;
            }

            return true;
        }
    }
}
