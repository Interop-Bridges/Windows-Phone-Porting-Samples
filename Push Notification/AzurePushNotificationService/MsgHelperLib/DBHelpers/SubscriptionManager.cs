// --------------------------------------------------------------------
// <copyright file="SubscriptionManager.cs" company="Microsoft Corp">
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
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using MsgHelperLib.Common;
    using MsgHelperLib.Model;

    /// <summary>
    /// Manages the device subscriptions and provides CRUD operations.
    /// </summary>
    public class SubscriptionManager
    {
        private SubscriptionDataSource sds;
        private DeviceManager deviceMgr;
        private SubscriptionInfoManager subInfoMgr;

        /// <summary>
        /// Initializes a new instance of the SubscriptionManager class.
        /// </summary>
        public SubscriptionManager()
        {
            this.sds = new SubscriptionDataSource();
            this.deviceMgr = new DeviceManager();
            this.subInfoMgr = new SubscriptionInfoManager();
        }

        /// <summary>
        /// Signup the device to the subscription.
        /// </summary>
        /// <param name="deviceId">The device id of the device being added.</param>
        /// <param name="subscription">Name of the subscription being signed up.</param>
        /// <returns>Returns success if the signup is successful. Error otherwise.</returns>
        public PushMessageError AddSubscription(string deviceId, string subscription)
        {
            PushMessageError err = this.Validate(deviceId, subscription);
            if (err != PushMessageError.Success)
            {
                return err;
            }

            SubscriptionDataModel sdm = this.sds.SelectByDeviceIdAndSubscription(deviceId, subscription);
            if (sdm == null)
            {
                sdm = new SubscriptionDataModel(subscription, deviceId);
                this.sds.Insert(sdm);
            }

            return PushMessageError.Success;
        }

        /// <summary>
        /// Delete the device signup.
        /// </summary>
        /// <param name="deviceId">Device id of the device to be deleted.</param>
        /// <param name="subscription">Subscription name.</param>
        /// <returns>Returns success if deletion is successful or error otherwise.</returns>
        public PushMessageError DeleteSubscription(string deviceId, string subscription)
        {
            PushMessageError err = this.Validate(deviceId, subscription);
            if (err != PushMessageError.Success)
            {
                return err;
            }

            SubscriptionDataModel sdm = this.sds.SelectByDeviceIdAndSubscription(deviceId, subscription);
            if (sdm != null)
            {
                // Delete it from the d/b
                this.sds.Delete(sdm);
                return PushMessageError.Success;
            }
            
            return PushMessageError.ErrorDeviceNotRegisteredForSubscription;
        }

        /// <summary>
        /// Returns all subscriptions in the system along with if the device has signed up for it.
        /// </summary>
        /// <param name="deviceId">The device id of the device to be searched.</param>
        /// <returns>Returns the list of device subscriptions.</returns>
        public Collection<DeviceSubscriptionInfo> GetDeviceSubscriptions(string deviceId)
        {
            // Initialize the list.
            Collection<DeviceSubscriptionInfo> devSubList = new Collection<DeviceSubscriptionInfo>();

            // Get all subscriptions in the system.
            Collection<SubscriptionInfo> subList = this.subInfoMgr.GetSubscriptionsInfo();
            foreach (SubscriptionInfo sub in subList)
            {
                DeviceSubscriptionInfo dsi = new DeviceSubscriptionInfo();
                dsi.Name = sub.Name;
                dsi.Description = sub.Description;
                SubscriptionDataModel sdm = this.sds.SelectByDeviceIdAndSubscription(deviceId, sub.Name);
                
                // If the sdm is null, device has not signed up
                if (sdm == null)
                {
                    dsi.IsSubscribed = "false";
                }
                else
                {
                    dsi.IsSubscribed = "true";
                }
                
                devSubList.Add(dsi);
            }
            
            return devSubList;
        }

        /// <summary>
        /// Delete the device subscriptions
        /// </summary>
        /// <param name="deviceId">Device ID being deleted</param>
        /// <returns>Returns success if successful or error otherwise.</returns>
        public PushMessageError DeleteDeviceSubscriptions(string deviceId)
        {
            Collection<DeviceSubscriptionInfo> subList = this.GetDeviceSubscriptions(deviceId);
            PushMessageError err;
            foreach (DeviceSubscriptionInfo sub in subList)
            {
                if ((err = this.DeleteSubscription(deviceId, sub.Name)) != PushMessageError.Success)
                {
                    return err;
                }
            }
            
            return PushMessageError.Success;
        }
        
        /// <summary>
        /// Gets the list details of devices that have signed up for the subscriptions.
        /// </summary>
        /// <param name="subId">Subscription name</param>
        /// <returns>Returns the list of device details.</returns>
        public Collection<DeviceInfo> GetDevices(string subId)
        {
            IEnumerable<SubscriptionDataModel> isdm = this.sds.SelectBySubscription(subId);
            Collection<DeviceInfo> devList = new Collection<DeviceInfo>();
            foreach (SubscriptionDataModel sdm in isdm)
            {
                DeviceInfo devInfo = new DeviceInfo();
                devInfo.DeviceId = sdm.DeviceId;
                DeviceDataModel ddm = this.deviceMgr.GetDevice(sdm.DeviceId);
                devInfo.DeviceType = ddm.DeviceType;
                devInfo.DeviceUri = ddm.Address;
                devList.Add(devInfo);
            }
            
            return devList;
        }

        /// <summary>
        /// Validates that device is in the system and subscription is in the system.
        /// </summary>
        /// <param name="deviceId">Device id of the device being checked.</param>
        /// <param name="subscriptionName">Subscription name being checked.</param>
        /// <returns>Returns an error if device or subscription is not found. Success otherwise.</returns>
        private PushMessageError Validate(string deviceId, string subscriptionName)
        {
            bool deviceRegistered = this.deviceMgr.IsDeviceRegistered(deviceId);
            if (!deviceRegistered)
            {
                return PushMessageError.ErrorDeviceNotFound;
            }

            bool subscriptionRegistered = this.subInfoMgr.IsSubscriptionRegistered(subscriptionName);
            if (!subscriptionRegistered)
            {
                return PushMessageError.ErrorSubscriptionNameNotFound;
            }

            return PushMessageError.Success;
        }

        public PushMessageError DeleteDeviceSubscriptionsBySubscription(string subId)
        {
            if (string.IsNullOrEmpty(subId))
            {
                return PushMessageError.ErrorSubscriptionNameNotFound;
            }

            bool subscriptionRegistered = this.subInfoMgr.IsSubscriptionRegistered(subId);
            if (!subscriptionRegistered)
            {
                return PushMessageError.ErrorSubscriptionNameNotFound;
            }

            IEnumerable<SubscriptionDataModel> isdm = this.sds.SelectBySubscription(subId);
            foreach (SubscriptionDataModel sdm in isdm)
            {
                this.DeleteSubscription(sdm.DeviceId, subId);
            }
            return PushMessageError.Success;

        }
    }
}
