// --------------------------------------------------------------------
// <copyright file="AndroidDevice.svc.cs" company="Microsoft Corp">
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
    /// AndroidDevice represents the android phones in the system.
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true,
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Single)]
    public class AndroidDevice : IAndroidService
    {
        private const string DeviceTypeName = "Android";
        private Push push;

        /// <summary>
        /// Initializes a new instance of the AndroidDevice class.
        /// </summary>
        public AndroidDevice()
        {
            this.push = new Push();
        }

        /// <summary>
        /// Registers an iOS device in the service.
        /// </summary>
        /// <param name="deviceId">The device id to register.</param>
        /// <returns>Return "success" or "error".</returns>
        public string RegisterDevice(string deviceId)
        {
            return this.push.RegisterDevice(deviceId, DeviceTypeName, string.Empty);
        }

        /// <summary>
        /// Unregisters an iOS device.
        /// </summary>
        /// Currently, this service is only allowed for admins. Devices won't unregister themselves. 
        /// They will remove their subscriptions. 
        /// <param name="deviceId">DeviceId represents the device id to unregister.</param>
        /// <returns>Returns "success" or "error".</returns>
        public string UnregisterDevice(string deviceId)
        {            
            if (!AuthManager.AuthenticateUser())
            {
                AuthManager.ConstructAuthResponse();
                return null;
            }

            return this.push.UnregisterDevice(deviceId);
        }
    }
}
