// --------------------------------------------------------------------
// <copyright file="AzureLogger.cs" company="Microsoft Corp">
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
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Used by Worker role to log connection errors to the Azure logging service. 
    /// You can use variety of ways to get the logs and identify the errors being encountered
    /// </summary>
    internal class AzureLogger
    {
        /// <summary>
        /// Event handler for logging APNS connection error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void ApnsConnectionError(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "APNS error: {0}", nea.Exc.Message));
        }

        /// <summary>
        /// Event handler for logging APNS DeviceID format error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void ApnsConnectionIllegalDeviceId(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "APNS error: Device ID too long. Error: {0}", nea.Exc.Message));
        }

        /// <summary>
        /// Event handler for Notification format errors. Logs them to Azure. 
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void ApnsConnectionNotificationFormatError(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "APNS error: Notification format error. Error: {0}", nea.Exc.Message));
        }

        /// <summary>
        /// Event handler for logging APNS notification failed errors
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void ApnsConnectionNotificationFailed(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            string mesg = nea.Exc.Message;
            if (nea.Exc.InnerException != null)
            {
                mesg += "(inner exc: " + nea.Exc.InnerException.Message + ")";
            }

            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "APNS error: Notification failed. Error: {0}", mesg));
        }

        /// <summary>
        /// Event handler for logging MPNS notification failed error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void MpnsConnectionNotificationFailed(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "MPNS error: Notification failed. Error: {0}", nea.Exc.Message));
        }

        /// <summary>
        /// Event handler for logging MPNS notification error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void MpnsConnectionNotificationError(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "MPNS error: Notification format error. Error: {0}", nea.Exc.Message));
        }

        /// <summary>
        /// Event handler for logging MPNS device id format error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void MpnsConnectionDeviceIdError(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "MPNS error: Device ID too long. Error: {0}", nea.Exc.Message));
        }

        /// <summary>
        /// Event handler for logging MPNS notification error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void MpnsConnectionError(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "MPNS error: {0}", nea.Exc.Message));
        }

        /// <summary>
        /// Event handler for logging C2DM (Google) notification failed error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void C2dmConnectionNotificationFailed(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "C2DM error: Notification failed. Error: {0}", nea.Exc.Message));
        }

        /// <summary>
        /// Event handler for logging C2DM (Google) notification  error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void C2dmConnectionNotificationError(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "C2DM error: Notification format error. Error: {0}", nea.Exc.Message));
        }

        /// <summary>
        /// Event handler for logging C2DM (Google) Device ID  error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void C2dmConnectionDeviceIdError(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "C2DM error: Device ID too long. Error: {0}", nea.Exc.Message));
        }

        /// <summary>
        /// Event handler for logging C2DM (Google) connection error
        /// </summary>
        /// <param name="sender">sender of the message</param>
        /// <param name="ea">Event argument for the event handler</param>
        public void C2dmConnectionError(object sender, EventArgs ea)
        {
            NotificationEventArgs nea = ea as NotificationEventArgs;
            System.Diagnostics.Trace.TraceError(string.Format(CultureInfo.InvariantCulture, "C2DM error: {0}", nea.Exc.Message));
        }
    }
}
