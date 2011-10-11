// --------------------------------------------------------------------
// <copyright file="WorkerRole.cs" company="Microsoft Corp">
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
namespace ApmWorkerRole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;
    using MsgHelperLib.Messages;
    using MsgHelperLib.Model;
    using MsgHelperLib.Queue;

    /// <summary>
    /// The Worker role that pushes messages to notification services.
    /// </summary>
    public sealed class WorkerRole : RoleEntryPoint, IDisposable
    {
        private const int WorkerRoleLoopTime = 10000;

        // Assumes it is in the same directory as your app
        private const string ApnsP12File = "apn_developer_identity.p12";

        // This is the password that you protected your p12File 
        // If you did not use a password, set it as null or an empty string
        private const string ApnsP12FilePassword = "YourAPNCertPwd";

        private PushMessageQueue msgQueue;
        private MpnsConnection mpnsConnection;
        private ApnsConnection apnsConnection;
        private C2dmConnection c2dmConnection;
        private AzureLogger logger;
        private string mpnsDevType;
        private Dictionary<PushMessageType, bool> mpnsSupportedMessages;
        private string apnsDevType;
        private Dictionary<PushMessageType, bool> apnsSupportedMessages;
        private string c2dmDevType;
        private Dictionary<PushMessageType, bool> c2dmSupportedMessages;

        private SubscriptionDataSource sds;

        /// <summary>
        /// Implements Dispose for IDisposable
        /// </summary>
        public void Dispose()
        {
            if (this.apnsConnection != null)
            {
                this.apnsConnection.Dispose();
            }
        }

        /// <summary>
        /// Obligator Run for the Azure worker role
        /// </summary>
        public override void Run()
        {
            Trace.WriteLine("APMWorkerRole run", "Information");

            // Retrieve a reference to the messages queue 
            this.msgQueue = new PushMessageQueue();

            Type enumType = typeof(MsgHelperLib.Messages.PushMessageType);

            // Create our connection objects to two services.
            // Create MPNSConnection to send the message immediately. Retry 3 times before logging error.
            this.mpnsConnection = new MpnsConnection(WP7BatchingPolicy.Immediately, 3);

            // Get the devicetype MPNS supports, only WP7
            this.mpnsDevType = this.mpnsConnection.SupportedDeviceType;

            // Get the type of messages it handles, toast, raw, tile and common
            this.mpnsSupportedMessages = this.mpnsConnection.HandlesMessageTypes(Enum.GetValues(enumType));

            // Create APNSConnection for Apple Push notification. Use Sandbox using certs and use 3 retries before logging error
            this.apnsConnection = new ApnsConnection(true, ApnsP12File, ApnsP12FilePassword, 3);

            // Get the type of device it support
            this.apnsDevType = this.apnsConnection.SupportedDeviceType;

            // Find the types of messages it supports. iPhone and Common types
            this.apnsSupportedMessages = this.apnsConnection.HandlesMessageTypes(Enum.GetValues(enumType));

            // Create APNSConnection for Apple Push notification. Use Sandbox using certs and use 3 retries before logging error
            this.c2dmConnection = new C2dmConnection(3);

            // Get the type of device it support
            this.c2dmDevType = this.c2dmConnection.SupportedDeviceType;

            // Find the types of messages it supports. iPhone and Common types
            this.c2dmSupportedMessages = this.c2dmConnection.HandlesMessageTypes(Enum.GetValues(enumType));

            // Create a new Azure logger and hook up all events the connections will raise. 
            this.logger = new AzureLogger();
            this.apnsConnection.NotificationError += this.logger.ApnsConnectionError;
            this.apnsConnection.DeviceIdFormatError += this.logger.ApnsConnectionIllegalDeviceId;
            this.apnsConnection.NotificationFormatError += this.logger.ApnsConnectionNotificationFormatError;
            this.apnsConnection.NotificationFailed += this.logger.ApnsConnectionNotificationFailed;

            this.mpnsConnection.NotificationError += this.logger.MpnsConnectionError;
            this.mpnsConnection.DeviceIdFormatError += this.logger.MpnsConnectionDeviceIdError;
            this.mpnsConnection.NotificationFormatError += this.logger.MpnsConnectionNotificationError;
            this.mpnsConnection.NotificationFailed += this.logger.MpnsConnectionNotificationFailed;

            this.c2dmConnection.NotificationError += this.logger.C2dmConnectionError;
            this.c2dmConnection.DeviceIdFormatError += this.logger.C2dmConnectionDeviceIdError;
            this.c2dmConnection.NotificationFormatError += this.logger.C2dmConnectionNotificationError;
            this.c2dmConnection.NotificationFailed += this.logger.C2dmConnectionNotificationFailed;

            this.sds = new SubscriptionDataSource();

            PushMessage pushMsg;
            DateTime lastMsgCheckTime = DateTime.Now;
            DateTime currentTime;

            // in the worker loop
            while (true)
            {
                // Sleep for the leftover time and dequeue a message and then process it. We sleep for at most "WorkerRoleLoopTime" in every cycle
                currentTime = DateTime.Now;
                int millSecForNextCheck = WorkerRoleLoopTime - currentTime.Subtract(lastMsgCheckTime).Milliseconds;
                Thread.Sleep(millSecForNextCheck > 0 ? millSecForNextCheck : 0);
                lastMsgCheckTime = DateTime.Now;
                if ((pushMsg = this.msgQueue.Deque()) != null)
                {
                    // Process will look at the message type and send it to appropriate connection
                    this.ProcessMessage(pushMsg);
                    Trace.WriteLine("APMWorkerRole Received message", "Information");
                }
            }
        }

        /// <summary>
        /// Obligatory OnStop for the Azure worker role
        /// </summary>
        /// <returns>Returns True if OnStart is successful.</returns>
        public override void OnStop()
        {
            if (apnsConnection != null)
            {
                apnsConnection.Close();
            }

            if (c2dmConnection != null)
            {
                c2dmConnection.Close();

            }

            if (mpnsConnection != null)
            {
                mpnsConnection.Close();
            }

        }

        /// <summary>
        /// Obligatory OnStart for the Azure worker role
        /// </summary>
        /// <returns>Returns True if OnStart is successful.</returns>
        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Configure our trace listener
            StartTraceListener();

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            // This code sets up a handler to update CloudStorageAccount instances when their corresponding
            // configuration settings change in the service configuration file.
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                // Provide the configSetter with the initial value
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));

                RoleEnvironment.Changed += (sender, arg) =>
                {
                    if (arg.Changes.OfType<RoleEnvironmentConfigurationSettingChange>()
                        .Any((change) => (change.ConfigurationSettingName == configName)))
                    {
                        // The corresponding configuration setting has changed, propagate the value
                        if (!configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)))
                        {
                            // In this case, the change to the storage account credentials in the
                            // service configuration is significant enough that the role needs to be
                            // recycled in order to use the latest settings. (for example, the 
                            // endpoint has changed)
                            RoleEnvironment.RequestRecycle();
                        }
                    }
                };
            });
            return base.OnStart();
        }

        private static void StartTraceListener()
        {
            TimeSpan oneMinuteTimeSpan = TimeSpan.FromMinutes(1);
            DiagnosticMonitorConfiguration dmc = DiagnosticMonitor.GetDefaultInitialConfiguration();

            // Transfer logs to storage every minute
            dmc.Logs.ScheduledTransferPeriod = oneMinuteTimeSpan;
            dmc.Logs.ScheduledTransferLogLevelFilter = LogLevel.Warning;

            // Transfer verbose, critical, etc. logs
            dmc.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;

            // Start up the diagnostic manager with the given configuration
            DiagnosticMonitor.Start("DiagnosticsConnectionString", dmc);
        }

        // Based on the type of message, it is sent to appropriate connection
        private void ProcessMessage(PushMessage pushMsg)
        {
            // Check if APNS and MPNS support a given type
            bool ifAPNSSupportsType = this.apnsSupportedMessages[(PushMessageType)pushMsg.MessageType];
            bool ifMPNSSupportsType = this.mpnsSupportedMessages[(PushMessageType)pushMsg.MessageType];
            bool ifC2DMSupportsType = this.c2dmSupportedMessages[(PushMessageType)pushMsg.MessageType];

            if (ifAPNSSupportsType)
            {
                // If APNS supports it, get the list of devices of the type APNS supports. 
                IEnumerable<DeviceDataModel> ddmList = this.sds.SelectByDeviceTypeAndSubscription(pushMsg.SubscriptionName, this.apnsDevType);
                this.apnsConnection.EnqueMessage(ddmList, pushMsg);
            }

            if (ifMPNSSupportsType)
            {
                // If MPNS supports the message type, get the list of devices of MPNS type and subscribed to a given subscription
                IEnumerable<DeviceDataModel> ddmList = this.sds.SelectByDeviceTypeAndSubscription(pushMsg.SubscriptionName, this.mpnsDevType);
                this.mpnsConnection.EnqueMessage(ddmList, pushMsg);
            }

            if (ifC2DMSupportsType)
            {
                // If APNS supports it, get the list of devices of the type APNS supports. 
                IEnumerable<DeviceDataModel> ddmList = this.sds.SelectByDeviceTypeAndSubscription(pushMsg.SubscriptionName, this.c2dmDevType);
                this.c2dmConnection.EnqueMessage(ddmList, pushMsg);
            }
        }
    }
}
