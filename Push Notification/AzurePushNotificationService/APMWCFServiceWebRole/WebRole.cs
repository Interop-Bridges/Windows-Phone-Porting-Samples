// --------------------------------------------------------------------
// <copyright file="WebRole.cs" company="Microsoft Corp">
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
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.Diagnostics.Management;
    using Microsoft.WindowsAzure.ServiceRuntime;

    /// <summary>
    /// WebRole class is the main web interface. Provides WCF web services for devices and messages.
    /// </summary>
    public class WebRole : RoleEntryPoint
    {
        /// <summary>
        /// OnStart is the main entry point of the role.
        /// </summary>
        /// <returns>Returns if OnStart is successful.</returns>
        public override bool OnStart()
        {
            // To enable the AzureLocalStorageTraceListner, uncomment relevent section in the web.config  
            StartTraceListener();

            System.Diagnostics.Trace.TraceInformation("Diagnostics Running");

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += this.RoleEnvironmentChanging;

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

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // If a configuration setting is changing
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }
    }
}
