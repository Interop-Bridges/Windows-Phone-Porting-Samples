// --------------------------------------------------------------------
// <copyright file="IPushService.cs" company="Microsoft Corp">
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
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;
    using MsgHelperLib.Common;
    using MsgHelperLib.Helpers;
    using MsgHelperLib.Model;

    /// <summary>
    /// Interface defines the service that is used to for sending push notifications and other API.
    /// </summary>
    [ServiceContract]
    public interface IPushService
    {
        /// <summary>
        /// List all subscriptions in the system. User may pick out of these.
        /// </summary>
        /// <returns>Returns a collection of subscriptions.</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/subs")]
        Collection<SubscriptionInfo> ListSubscriptions();

        /// <summary>
        /// Create a subscriptions in the system. 
        /// </summary>
        /// <param name="subId">New subscription to be created.</param>
        /// <param name="description">Description of the subscription to be created.</param>
        /// <returns>Returns "success" or an error.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/sub/create/{subId}?desc={description}")]
        string CreateSubscription(string subId, string description);

        /// <summary>
        /// Delete a subscriptions in the system. 
        /// </summary>
        /// <param name="subId">New subscription to be created.</param>
        /// <param name="description">Description of the subscription to be created.</param>
        /// <returns>Returns "success" or an error.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/sub/delete/{subId}")]
        string DeleteSubscription(string subId);

        /// <summary>
        /// List all subscriptions device has signed up for.
        /// </summary>
        /// <param name="deviceId">The device id of the device being checked.</param>
        /// <returns>Returns a list of subscriptions and whether device has signed up for each. </returns>
        [OperationContract]
        [WebGet(UriTemplate = "/subs/{deviceId}")]
        Collection<DeviceSubscriptionInfo> ListDeviceSubscriptions(string deviceId);

        /// <summary>
        /// Sign up a device for a particular subscription.
        /// </summary>
        /// <param name="subId">Subscription id to sign up for.</param>
        /// <param name="deviceId">The device id of the device being checked.</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "/sub/add/{subId}/{deviceId}")]
        void AddDeviceSubscription(string subId, string deviceId);

        /// <summary>
        /// Remove a device subscription.
        /// </summary>
        /// <param name="subId">Subscription id to remove.</param>
        /// <param name="deviceId">The device id of the device.</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "/sub/delete/{subId}/{deviceId}")]
        void DeleteDeviceSubscription(string subId, string deviceId);

        /// <summary>
        /// Get all devices signed up for a subscription up for.
        /// </summary>
        /// <param name="subId">Subscription id to check.</param>
        /// <returns>Returns a collection of DeviceInfo that have signed up for a subscription.</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/sub/{subId}")]
        Collection<DeviceInfo> ListSubscriptionDevices(string subId);

        /// <summary>
        /// Send a toast to WP7 devices signed up for a particular subscription.
        /// </summary>
        /// <param name="subId">Subscription id to send a message to.</param>
        /// <param name="message">Toast message text.</param>
        /// <returns>Returns "success" or an error.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/message/toast/{subId}?mesg={message}")]
        string SendToastNotification(string subId, string message);

        /// <summary>
        /// Send a raw notification to WP7 devices signed up for a particular subscription.
        /// </summary>
        /// <param name="subId">Subscription id to send a message to.</param>
        /// <param name="message">Raw message text.</param>
        /// <returns>Returns "success" or an error.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/message/raw/{subId}?mesg={message}")]
        string SendRawNotification(string subId, string message);

        /// <summary>
        /// Send a tile to WP7 devices signed up for a particular subscription.
        /// </summary>
        /// <param name="subId">Subscription id to send a message to.</param>
        /// <param name="message">Tile message title.</param>
        /// <param name="count">Tile message count to be updated.</param>
        /// <param name="image">Tile message image to be updated.</param>
        /// <returns>Returns "success" or an error.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/message/tile/{subId}?mesg={message}&count={count}&img={image}")]
        string SendTileNotification(string subId, string message, string count, string image);

        /// <summary>
        /// Send a notification to iOS devices signed up for a particular subscription.
        /// </summary>
        /// <param name="subId">Subscription id to send a message to.</param>
        /// <param name="message">IOS message caption.</param>
        /// <param name="count">IOS message count to be updated.</param>
        /// <param name="sound">IOS message sound to be played.</param>
        /// <returns>Returns "success" or an error.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/message/iOS/{subId}?mesg={message}&count={count}&alert={sound}")]
        string SendIphoneNotification(string subId, string message, string count, string sound);

        /// <summary>
        /// Send a notification to iOS devices signed up for a particular subscription.
        /// </summary>
        /// <param name="subId">Subscription id to send a message to.</param>
        /// <param name="message">Message caption.</param>
        /// <param name="count">Message count to be updated.</param>
        /// <param name="image">Message image to be updated.</param>
        /// <param name="sound">Message sound to be played.</param>
        /// <returns>Returns "success" or an error.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/message/common/{subId}?mesg={message}&count={count}&img={image}&alert={sound}")]
        string SendCommonNotification(string subId, string message, string count, string image, string sound);
    }
}
