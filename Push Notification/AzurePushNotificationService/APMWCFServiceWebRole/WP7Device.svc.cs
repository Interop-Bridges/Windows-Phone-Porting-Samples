// --------------------------------------------------------------------
// <copyright file="WP7Device.svc.cs" company="Microsoft Corp">
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

    // implementation for the WP7 device registration end point

    /// <summary>
    /// Implementation of WP7 device registrarion / deregistration service endpoint. 
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true,
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Single)]
    public class WP7Device : IWP7Service
    {
        private const string DeviceTypeName = "WP7";

        private Push push;

        /// <summary>
        /// Initializes a new instance of the WP7Device class.
        /// </summary>
        /// initialize all our managers
        public WP7Device()
        {
            this.push = new Push();
        }

        /// <summary>
        /// Registers a device in the service.
        /// </summary>
        /// <param name="deviceId">The id of the WP7 device.</param>
        /// <param name="deviceUri">URI of the device being registered.</param>
        /// <returns>Returns "success" if registration is successful otherwise raises an exception.</returns>
        public string RegisterDevice(string deviceId, string deviceUri)
        {
            try
            {
                if (IsDeviceIdValid(deviceId) && IsUriValid(ref deviceUri))
                {
                    return this.push.RegisterDevice(deviceId, DeviceTypeName, deviceUri);
                }
                else
                {
                    throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorIllegalDeviceId), HttpStatusCode.BadRequest);
                }
            }
            catch
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorIllegalDeviceId), HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// To unregister a WP7 device from the system.
        /// </summary>
        /// <param name="deviceId">Device id of the device being unregistered.</param>
        /// <returns>Returns "success" if successful or an exception.</returns>
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

        // There is no constraint on the WP7 Device ID. Make sure blank deviceID is not sent. everything else should be okay. 
        private static bool IsDeviceIdValid(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                return false;
            }

            return true;
        }

        // We encode the URI since it has '/' characters that REST does not like
        private static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes
                = System.Convert.FromBase64String(encodedData);
            string returnValue =
                System.Text.UTF8Encoding.UTF8.GetString(encodedDataAsBytes);
            return returnValue;
        }

        // Check if the URI is legal. encoded paramter tells if it is encoded. for non REST, encoded could be false
        // We decode and return the decoded URI
        private static bool IsUriValid(ref string uri)
        {
            uri = DecodeFrom64(uri);
            return Uri.IsWellFormedUriString(uri, UriKind.Absolute);
        }
    }
}
