// --------------------------------------------------------------------
// <copyright file="SubscriptionInfoManager.cs" company="Microsoft Corp">
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
    /// A helper class to manage subscription info records in the system. Persists them in the azure tables with simple API
    /// </summary>
    public class SubscriptionInfoManager
    {
        private SubscriptionInfoDataSource sids;
        
        /// <summary>
        /// Initializes a new instance of the SubscriptionInfoManager class.
        /// </summary>
        public SubscriptionInfoManager()
        {
            this.sids = new SubscriptionInfoDataSource();
        }

        /// <summary>
        /// Return true if the subscription name is in the d/b.
        /// </summary>
        /// <param name="subscriptionName">Subscription name to check.</param>
        /// <returns>Returns true if subscription name is in the d/b. False otherwise.</returns>
        public bool IsSubscriptionRegistered(string subscriptionName)
        {
            SubscriptionInfoDataModel sidm = this.sids.SelectBySubscription(subscriptionName);
            if (sidm == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Adds a subscription with a name and its description
        /// </summary>
        /// <param name="subscription">Name of the subscription.</param>
        /// <param name="description">Descripton of the subscription.</param>
        /// <returns>Returns Success or an error.</returns>
        public PushMessageError AddSubscriptionInfo(string subscription, string description)
        {
            // See if we already have this subscription.
            SubscriptionInfoDataModel sidm = this.sids.SelectBySubscription(subscription);
            if (sidm == null)
            {
                    sidm = new SubscriptionInfoDataModel(subscription, description);
                    this.sids.Insert(sidm);
                    return PushMessageError.Success;
            }

            // We are allowing the same subscription again. It is ignored. 
            return PushMessageError.Success;
        }

        /// <summary>
        /// Delete the subscription given its name
        /// Does not clean up the devices that have signed up. 
        /// Clean up devices before deleting subscription info.
        /// </summary>
        /// <param name="subscription">Name of the subscription to delete.</param>
        public PushMessageError DeleteSubscriptionInfo(string subscription)
        {
            if (string.IsNullOrEmpty(subscription))
            {
                return PushMessageError.ErrorSubscriptionNameNotFound;
            }

            // Eetrieve the subscription from the system. We are not cleaning signups here. 
            SubscriptionInfoDataModel sidm = this.sids.SelectBySubscription(subscription);
            if (sidm != null)
            {
                this.sids.Delete(sidm);
                return PushMessageError.Success;
            }
            else
            {
                return PushMessageError.ErrorInternalError;
            }
        }

        /// <summary>
        /// Retrieve a subscription from the d/b.
        /// </summary>
        /// <param name="subscription">Name of the subscription to search.</param>
        /// <returns>Return the subscription details: name and descriptions.</returns>
        public SubscriptionInfoDataModel GetSubscriptionInfo(string subscription)
        {
            SubscriptionInfoDataModel sidm = this.sids.SelectBySubscription(subscription);
            return sidm;
        }

        /// <summary>
        /// Provides the list of subscriptions, name and description pairs
        /// </summary>
        /// <returns>Returns the list of subscriptions in the system.</returns>
        public Collection<SubscriptionInfo> GetSubscriptionsInfo()
        {
            IEnumerable<SubscriptionInfoDataModel> subs = this.sids.Subscriptions();
            Collection<SubscriptionInfo> subsList = new Collection<SubscriptionInfo>();
            foreach (SubscriptionInfoDataModel sub in subs)
            {
                // Here we are not worried about whether the subscription is signed up or not. Just returning the list.
                SubscriptionInfo s = new SubscriptionInfo() 
                { 
                    Name = sub.SubscriptionName, Description = sub.Description 
                };
                subsList.Add(s);
            }

            return subsList;
        }
    }
}
