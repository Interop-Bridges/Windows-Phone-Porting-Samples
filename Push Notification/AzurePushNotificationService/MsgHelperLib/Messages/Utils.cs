// --------------------------------------------------------------------
// <copyright file="Utils.cs" company="Microsoft Corp">
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
namespace MsgHelperLib.Messages
{
    /// <summary>
    /// Lists the types of notifications
    /// </summary>
    public enum WP7NotificationType
    {
        /// <summary>
        /// This value is not used. 
        /// </summary>
        None = 0,

        /// <summary>
        /// This is used to represent the tile message.
        /// </summary>
        Tile = 1,

        /// <summary>
        /// This is used to denote the toast message.
        /// </summary>
        Toast = 2,

        /// <summary>
        /// This is used to denote the raw message.
        /// </summary>
        Raw = 3
    }

    /// <summary>
    /// WP7 batching policy.
    /// </summary>
    public enum WP7BatchingPolicy
    {
        /// <summary>
        /// This value is not used.
        /// </summary>
        None = 0,

        /// <summary>
        /// Denotes the policy to send message immediately.
        /// </summary>
        Immediately = 1,

        /// <summary>
        /// Denotes the policy to send message with 450ms wait.
        /// </summary>
        Wait450 = 2,

        /// <summary>
        /// Denotes the policy to send message with 900ms wait.
        /// </summary>
        Wait900 = 3
    }
}