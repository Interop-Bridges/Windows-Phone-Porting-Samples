// --------------------------------------------------------------------
// <copyright file="IWP7Service.cs" company="Microsoft Corp">
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
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;
    using MsgHelperLib.Helpers;
    using MsgHelperLib.Model;

    /// <summary>
    /// Interface defines the service that is used to for registering WP7 devices.
    /// </summary>
    [ServiceContract]
    public interface IWP7Service
    {
        /// <summary>
        /// Registers a device in the notification service.
        /// </summary>
        /// <param name="deviceId">The device id of the device to be registered.</param>
        /// <param name="deviceUri">The URI of the WP7 device being registered.</param>
        /// <returns>Returns "success" or error.</returns>
        [WebInvoke(UriTemplate = "/register/{deviceId}?uri={deviceUri}")]
        string RegisterDevice(string deviceId, string deviceUri);

        /// <summary>
        /// Delete a device which is regisered. Removes all subscriptions associated with it.
        /// </summary>
        /// <param name="deviceId">The device id of the device to be unregistered.</param>
        /// <returns>Returns "success" or error.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/unregister/{deviceId}")]
        string UnregisterDevice(string deviceId);
    }
}
