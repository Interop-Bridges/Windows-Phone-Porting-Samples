// --------------------------------------------------------------------
// <copyright file="Push.svc.cs" company="Microsoft Corp">
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
    using System.Collections.ObjectModel;
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
    /// Push class is the Push Service that is used to enque messages, create subscriptions.
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true,
    InstanceContextMode = InstanceContextMode.PerSession,
    ConcurrencyMode = ConcurrencyMode.Single)]
    public class Push : IPushService
    {
        // Manages devices in the service.
        private DeviceManager devMgr;

        // Manages device subscriptions.
        private SubscriptionManager subscriptionMgr;

        // Send messages to the worker role for sending out.
        private PushMessageQueue msgQueue;

        // Manages subscriptions in the service.
        private SubscriptionInfoManager subscriptionInfoMgr;

        /// <summary>
        /// Initializes a new instance of the Push class.
        /// </summary>
        /// Initializes various data management classes.
        public Push()
        {
            this.devMgr = new DeviceManager();
            this.subscriptionMgr = new SubscriptionManager();
            this.msgQueue = new PushMessageQueue();
            this.subscriptionInfoMgr = new SubscriptionInfoManager();
        }

        /// <summary>
        /// Registers a device in the service.
        /// </summary>
        /// <param name="deviceId">The id of the device being registered.</param>
        /// <param name="type">The type of the device.</param>
        /// <param name="deviceUri">The URI of the device. Used only for WP7. empty string for others.</param>
        /// <returns>Returns "success" if successful or error otherwise.</returns>
        public string RegisterDevice(string deviceId, string type, string deviceUri)
        {
            PushMessageError err;

            // the device endpoints have made sure that device IDs are valid. This method is not visible to outside. 
            try
            {
                err = this.devMgr.AddOrUpdateDevice(type, deviceId, deviceUri);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: RegisterDevice  device: {0}, type:{1}, uri:{2}, Error: {3}", deviceId, type, deviceUri, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }

            // If there was error in adding the device, return error. 
            if (err != PushMessageError.Success)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(err), HttpStatusCode.BadRequest);
            }
            else
            {
                return "Success";
            }
        }

        /// <summary>
        /// Unregister this device from the service. This method is not accessible from outside. Device endpoints will use this method.
        /// </summary>
        /// <param name="deviceId">DeviceId of the device to unregister.</param>
        /// <returns>Returns "success" if successful in unregistering. Error otherwise. </returns>
        public string UnregisterDevice(string deviceId)
        {
            PushMessageError err;

            try
            {
                // Make sure device is registered with us.
                bool isRegistered = this.devMgr.IsDeviceRegistered(deviceId);
                if (!isRegistered)
                {
                    throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorDeviceNotFound), System.Net.HttpStatusCode.BadRequest);
                }

                // Delete device subscriptions.
                err = this.subscriptionMgr.DeleteDeviceSubscriptions(deviceId);
                if (err != PushMessageError.Success)
                {
                    // If error, throw an exception.
                    Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: UnregisterDevice device: {0}, Error: {1}", deviceId, Utils.GetErrorString(err)));
                    throw new WebFaultException<string>(Utils.GetErrorString(err), System.Net.HttpStatusCode.InternalServerError);
                }

                // Remove the device from the service. 
                err = this.devMgr.DeleteDevice(deviceId);
                if (err != PushMessageError.Success)
                {
                    // If error, throw an exception.
                    Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: UnregisterDevice device: {0}, Error: {1}", deviceId, Utils.GetErrorString(err)));
                    throw new WebFaultException<string>(Utils.GetErrorString(err), System.Net.HttpStatusCode.InternalServerError);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: UnRegisterDevice  device: {0}, Error: {1}", deviceId, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }

            return "Success";
        }

        /// <summary>
        /// This provides only the list of name, description pair for the subscriptions.
        /// </summary>
        /// <returns>Returns the list of Subscriptions.</returns>
        public Collection<SubscriptionInfo> ListSubscriptions()
        {
            Trace.WriteLine("WCF Service ListSubscriptions", "REST");
            try
            {
                // Get all subscriptions in the service.
                return this.subscriptionInfoMgr.GetSubscriptionsInfo();
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: ListSubscriptions Error: {0}", e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create a new subscription with the name and description.
        /// </summary>
        /// <param name="subId">The name of the subscription being created.</param>
        /// <param name="description">Description of the subscription.</param>
        /// <returns>Returns "success" if successful in creating subscription.</returns>
        public string CreateSubscription(string subId, string description)
        {
            // User must be authenticated.
            if (!AuthManager.AuthenticateUser())
            {
                // If not, return 401.
                AuthManager.ConstructAuthResponse();
                return null;
            }

            PushMessageError err;
            try
            {
                // Ask subscription manager to add one more subscription name.
                err = this.subscriptionInfoMgr.AddSubscriptionInfo(subId, description);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: CreateSubscription subName: {0}, Error: {1}", subId, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }

            if (err != PushMessageError.Success)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(err), System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                return "success";
            }
        }

        /// <summary>
        /// Delete a subscription with the given name.
        /// </summary>
        /// <param name="subId">The name of the subscription being created.</param>
        /// <returns>Returns "success" if successful in creating subscription.</returns>
        public string DeleteSubscription(string subId)
        {
            // User must be authenticated.
            if (!AuthManager.AuthenticateUser())
            {
                // If not, return 401.
                AuthManager.ConstructAuthResponse();
                return null;
            }

            PushMessageError err;
            try
            {
                // Ask subscription manager to delete the subscription.
                err = this.subscriptionMgr.DeleteDeviceSubscriptionsBySubscription(subId);
                if (err != PushMessageError.Success)
                {
                    throw new WebFaultException<string>(Utils.GetErrorString(err), System.Net.HttpStatusCode.BadRequest);
                }
                err = this.subscriptionInfoMgr.DeleteSubscriptionInfo(subId);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: DeleteSubscription subName: {0}, Error: {1}", subId, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }

            if (err != PushMessageError.Success)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(err), System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                return "success";
            }
        }

        /// <summary>
        /// Returns subscription info, name and description, and if the device has signed up for it. 
        /// </summary>
        /// <param name="deviceId">Device Id of the device being searched.</param>
        /// <returns>Returns collection of device subscriptions.</returns>
        public Collection<DeviceSubscriptionInfo> ListDeviceSubscriptions(string deviceId)
        {
            // Make sure device is registered with us.
            bool isRegistered = this.devMgr.IsDeviceRegistered(deviceId);
            if (!isRegistered)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorDeviceNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            // Now get the list of subscriptions device is signed up for.
            try
            {
                return this.subscriptionMgr.GetDeviceSubscriptions(deviceId);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: ListDeviceSubscriptions {0}, Error: {1}", deviceId, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Subscribes a device to a subscription.
        /// </summary>
        /// <param name="subId">Subscription id of the subscription.</param>
        /// <param name="deviceId">The id of the device that is signed up for the subscription.</param>
        public void AddDeviceSubscription(string subId, string deviceId)
        {
            Trace.TraceInformation(String.Format(CultureInfo.InvariantCulture, "WCF Service: WP7 AddDeviceSubscription - subscription {0} device : {1}   ", subId, deviceId));

            // Make sure device is registered with us.
            bool isRegistered = this.devMgr.IsDeviceRegistered(deviceId);
            if (!isRegistered)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorDeviceNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            // Make sure subscription name is created.
            bool subExists = this.subscriptionInfoMgr.IsSubscriptionRegistered(subId);
            if (!subExists)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorSubscriptionNameNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            // Now sign up this device for the given subscription.
            PushMessageError err;
            try
            {
                err = this.subscriptionMgr.AddSubscription(deviceId, subId);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: AddDeviceSubscription subscription: {0} device: {1}, Error: {2}", subId, deviceId, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }

            if (err != PushMessageError.Success)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(err), System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Deletes device subscription.
        /// </summary>
        /// <param name="subId">The subscription id of the subscription.</param>
        /// <param name="deviceId">The device id of the device being unsubscribed.</param>
        public void DeleteDeviceSubscription(string subId, string deviceId)
        {
            bool isRegistered = this.devMgr.IsDeviceRegistered(deviceId);
            PushMessageError err;

            if (!isRegistered)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorDeviceNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            // Make sure subscription name is created.
            bool subExists = this.subscriptionInfoMgr.IsSubscriptionRegistered(subId);
            if (!subExists)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorSubscriptionNameNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            // Now delete the device subscription.
            try
            {
                err = this.subscriptionMgr.DeleteSubscription(deviceId, subId);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: DeleteDeviceSubscription subscription: {0} device: {1}, Error: {2}", subId, deviceId, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }

            if (err != PushMessageError.Success)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(err), System.Net.HttpStatusCode.BadRequest);
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Lists all devices subscribed to a particular subscription.
        /// </summary>
        /// <param name="subId">Subscription id being searched.</param>
        /// <returns>Returns a collection of devices that are signed up to the subscription.</returns>
        public Collection<DeviceInfo> ListSubscriptionDevices(string subId)
        {
            if (!AuthManager.AuthenticateUser())
            {
                AuthManager.ConstructAuthResponse();
                return null;
            }

            // Make sure subscription name is created.
            bool subExists = this.subscriptionInfoMgr.IsSubscriptionRegistered(subId);
            if (!subExists)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorSubscriptionNameNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                return this.subscriptionMgr.GetDevices(subId);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: DeleteDeviceSubscription subscription: {0}  Error: {1}", subId, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Send a toast notification to all WP7 devices subscribed to a given subscription.
        /// </summary>
        /// <param name="subId">Id of the subscription to send the message to.</param>
        /// <param name="message">The message text.</param>
        /// <returns>Returns "success" if successful otherwise an error.</returns>
        public string SendToastNotification(string subId, string message)
        {
            if (!AuthManager.AuthenticateUser())
            {
                AuthManager.ConstructAuthResponse();
                return null;
            }

            // Make sure subscription name is created.
            bool subExists = this.subscriptionInfoMgr.IsSubscriptionRegistered(subId);
            if (!subExists)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorSubscriptionNameNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                ToastMessage toastMsg = new ToastMessage(subId, message);
                this.msgQueue.Enque(toastMsg);
                return "success";
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: SendToast subscription: {0} toast: {1}, Error: {2}", subId, message, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Send a Raw notification to all WP7 devices subscribed to a given subscription.
        /// </summary>
        /// <param name="subId">Id of the subscription to send the message to.</param>
        /// <param name="message">The message text.</param>
        /// <returns>Returns "success" if successful otherwise an error.</returns>
        public string SendRawNotification(string subId, string message)
        {
            // Make sure user is authenticated,
            if (!AuthManager.AuthenticateUser())
            {
                AuthManager.ConstructAuthResponse();
                return null;
            }

            // Make sure subscription name is created
            bool subExists = this.subscriptionInfoMgr.IsSubscriptionRegistered(subId);
            if (!subExists)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorSubscriptionNameNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                RawMessage rawMsg = new RawMessage(subId, message);
                this.msgQueue.Enque(rawMsg);
                return "success";
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: SendRaw subscription: {0} message: {1}, Error: {2}", subId, message, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Send a common notification to all WP7 and iPhone devices subscribed to a given subscription.
        /// </summary>
        /// <param name="subId">Id of the subscription to send the message to.</param>
        /// <param name="message">The message text.</param>
        /// <param name="count">Count to show on the tile or the badge.</param>
        /// <param name="image">The image to show on the tile.</param>
        /// <param name="sound">Sound file for the iPhone notification.</param>
        /// <returns>Returns "success" if successful otherwise an error.</returns>
        public string SendCommonNotification(string subId, string message, string count, string image, string sound)
        {
            // Make sure user is authenticated.
            if (!AuthManager.AuthenticateUser())
            {
                AuthManager.ConstructAuthResponse();
                return null;
            }

            // Make sure subscription name is created.
            bool subExists = this.subscriptionInfoMgr.IsSubscriptionRegistered(subId);
            if (!subExists)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorSubscriptionNameNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            // Make sure that the count is an int.
            int countVal;
            bool success = int.TryParse(count, out countVal);
            if (!success)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorIllegalCount), System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                CommonMessage commonMsg = new CommonMessage(subId, message, countVal, image, sound);
                this.msgQueue.Enque(commonMsg);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: SendCommon subscription: {0} title: {1}, Error: {2}", subId, message, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }

            return "success";
        }

        /// <summary>
        /// Send a tile notification to all WP7 devices subscribed to a given subscription.
        /// </summary>
        /// <param name="subId">Id of the subscription to send the message to.</param>
        /// <param name="message">The message text.</param>
        /// <param name="count">Count to show on the tile or the badge.</param>
        /// <param name="image">The image to show on the tile.</param>
        /// <returns>Returns "success" if successful otherwise an error.</returns>
        public string SendTileNotification(string subId, string message, string count, string image)
        {
            // Make sure user is authenticated.
            if (!AuthManager.AuthenticateUser())
            {
                AuthManager.ConstructAuthResponse();
                return null;
            }

            // Make sure subscription name is created.
            bool subExists = this.subscriptionInfoMgr.IsSubscriptionRegistered(subId);
            if (!subExists)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorSubscriptionNameNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            // Make sure that the count is an int.
            int countVal;
            bool success = int.TryParse(count, out countVal);
            if (!success)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorIllegalCount), System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                TileMessage tileMsg = new TileMessage(subId, message, count, image);
                this.msgQueue.Enque(tileMsg);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: SendTile subscription: {0} title: {1}, Error: {2}", subId, message, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }

            return "success";
        }

        /// <summary>
        /// Send a alert notification to all iPhone devices subscribed to a given subscription.
        /// </summary>
        /// <param name="subId">Id of the subscription to send the message to.</param>
        /// <param name="message">The message text.</param>
        /// <param name="count">Number to show on the badge.</param>
        /// <param name="sound">Sound file for the iPhone notification.</param>
        /// <returns>Returns "success" if successful otherwise an error.</returns>        
        public string SendIphoneNotification(string subId, string message, string count, string sound)
        {
            // Make sure user is authenticated.
            if (!AuthManager.AuthenticateUser())
            {
                AuthManager.ConstructAuthResponse();
                return null;
            }

            // Make sure subscription name is created.
            bool subExists = this.subscriptionInfoMgr.IsSubscriptionRegistered(subId);
            if (!subExists)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorSubscriptionNameNotFound), System.Net.HttpStatusCode.BadRequest);
            }

            // Make sure that the count is an int.
            int countVal;
            bool success = int.TryParse(count, out countVal);
            if (!success)
            {
                throw new WebFaultException<string>(Utils.GetErrorString(PushMessageError.ErrorIllegalCount), System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                iPhoneMessage iPhoneMsg = new iPhoneMessage(subId, message, count, sound);
                this.msgQueue.Enque(iPhoneMsg);
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "Internal Error: SendiOSAlert subscription: {0} title: {1}, Error: {2}", subId, message, e.Message));
                throw new WebFaultException<string>(e.Message, System.Net.HttpStatusCode.InternalServerError);
            }

            return "success";
        }
    }
}
