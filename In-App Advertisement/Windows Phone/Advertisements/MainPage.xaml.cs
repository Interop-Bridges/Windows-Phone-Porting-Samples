/*
Copyright 2011 Microsoft Corporation

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace Advertisements
{
	public partial class MainPage : PhoneApplicationPage
	{
		// NOTE: Use your real Application ID and Ad Unit ID here.
		private const string AD_CENTER_APPLICATION_ID = "3e4e070a-e1f5-46ac-abe9-ff2d21251cc5";
		private const string AD_CENTER_AD_UNIT_ID = "69777";

		/// <summary>
		/// Timer used to manually refresh the ad.
		/// </summary>
		private DispatcherTimer timer;

		/// <summary>
		/// Constructor
		/// </summary>
		public MainPage()
		{
			InitializeComponent();

			// setup the ad control
			adControl.ApplicationId = AD_CENTER_APPLICATION_ID;
			adControl.AdUnitId = AD_CENTER_AD_UNIT_ID;
			adControl.IsAutoCollapseEnabled = true;
			adControl.IsAutoRefreshEnabled = false;
			adControl.AdRefreshed += new EventHandler(adControl_AdRefreshed);

			// setup the timer to know when to manually refresh the ad
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(30);
			timer.Tick += new EventHandler(timer_Tick);
		}

		/// <summary>
		/// Event that is raised when the AdControl receives a new ad.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void adControl_AdRefreshed(object sender, EventArgs e)
		{
			// Now that the ad is refreshed we can start the timer.
			timer.Start();
		}

		/// <summary>
		/// Event that occurs when the timer interval has elapsed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void timer_Tick(object sender, EventArgs e)
		{
			// Stop the timer and refresh the ad.
			timer.Stop();
			adControl.Refresh();

			// NOTE: Waiting until after the ad is refreshed before restarting the timer.
		}
	}
}