// --------------------------------------------------------------------
// <copyright file="CommonTypes.cs" company="Microsoft Corp">
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
namespace MsgHelperLib.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    /// <summary>
    /// these are utility classes amd enums. All errors service can return
    /// </summary>
    public enum PushMessageError
    {
        /// <summary>
        /// No error. This is also needed for fxcop.
        /// </summary>
        Success = 0,

        /// <summary>
        /// DeviceId was illegal.
        /// </summary>
        ErrorIllegalDeviceId = 2,

        /// <summary>
        /// DeviceId is already registered. Currently this is not flagged as error.
        /// </summary>
        ErrorDeviceAlreadyRegistered = 3,

        /// <summary>
        /// Raised when a device is unscribed or a message is sent to a device which is not registered.
        /// </summary>
        ErrorDeviceNotFound = 4,

        /// <summary>
        /// Subscription is already subscribed. 
        /// </summary>
        ErrorSubscriptionNameAlreadyRegistered = 5,

        /// <summary>
        /// Subscription is not found. It was not created or already deleted. 
        /// </summary>
        ErrorSubscriptionNameNotFound = 6,

        /// <summary>
        /// Raised if a device does not signup for a particular subscription.
        /// </summary>
        ErrorDeviceNotRegisteredForSubscription = 7,

        /// <summary>
        /// Illegal value for the count field.
        /// </summary>
        ErrorIllegalCount = 8,

        /// <summary>
        /// This is not returned.  Only used for internal errors. 
        /// </summary>
        ErrorIllegalDeviceType = 9,

        /// <summary>
        /// Device URI format is illegal.
        /// </summary>
        ErrorIllegalUri = 10,

        /// <summary>
        /// An internal error.
        /// </summary>
        ErrorInternalError = 99,
    }

    /// <summary>
    /// Get the error strings for each of these errors. These are returned along with the exceptions
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Returns the error string for a given eror. 
        /// </summary>
        /// <param name="err">For the PushMessageError err.</param>
        /// <returns>Returns a string representation of the error.</returns>
        public static string GetErrorString(PushMessageError err)
        {
            switch (err)
            {
                case PushMessageError.Success:
                    return "success";
                case PushMessageError.ErrorDeviceAlreadyRegistered:
                    return "Device already registered.";
                case PushMessageError.ErrorDeviceNotFound:
                    return "Device not found.";
                case PushMessageError.ErrorIllegalDeviceId:
                    return "Device ID syntax incorrect.";
                case PushMessageError.ErrorInternalError:
                    return "Internal error.";
                case PushMessageError.ErrorSubscriptionNameAlreadyRegistered:
                    return "Subscription name already registered.";
                case PushMessageError.ErrorSubscriptionNameNotFound:
                    return "Subscription Name not found.";
                case PushMessageError.ErrorDeviceNotRegisteredForSubscription:
                    return "Device not registed for service.";
                case PushMessageError.ErrorIllegalCount:
                    return "Count value incorrect.";
                case PushMessageError.ErrorIllegalDeviceType:
                    return "Device type not recognized.";
                case PushMessageError.ErrorIllegalUri:
                    return "URI syntax incorrect.";
            }

            return "Success";
        }
    }

    /// <summary>
    /// Used to represent the basic subscription info,i.e. name of the subscription and its description.
    /// </summary>
    public class SubscriptionInfo
    {
        /// <summary>
        /// Gets or sets the name of the subscription.
        /// </summary>
        public string Name
        {            
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the description of the subscription.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Used to represent device subscribing to a subscription.
    /// </summary>
    public class DeviceSubscriptionInfo
    {
        /// <summary>
        /// Gets or sets the name of the device subscription.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the subscription.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets whether the device has signed up for this subscription.
        /// </summary>
        public string IsSubscribed { get; set; }
    }

    /// <summary>
    /// Used to represent the devices, id, type and the URI. 
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// Gets or sets  the Device id.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the device.
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// Gets or sets the URL of the device, if needed.
        /// </summary>
        public string DeviceUri { get; set; }
    }
}