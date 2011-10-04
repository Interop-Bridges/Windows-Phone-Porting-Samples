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
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;
using Microsoft.Phone.Controls.Maps.Core;
using Microsoft.Phone.Shell;

namespace GeoLocation
{
	public partial class MainPage : PhoneApplicationPage
	{
		private const int MOVEMENT_THRESHOLD_IN_METERS = 10;
		private GeoCoordinateWatcher _geoWatcher;
		private Pushpin _pin;
		private Pushpin _me;

		/// <summary>
		/// The view-model for this page.
		/// </summary>
		public MainPageViewModel ViewModel
		{
			get { return DataContext as MainPageViewModel; }
			set { DataContext = value; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public MainPage()
		{
			InitializeComponent();

			// Initialize the view-model.
			ViewModel = new MainPageViewModel();

			// Setup the GeoCordinateWatcher.
			_geoWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
			_geoWatcher.MovementThreshold = MOVEMENT_THRESHOLD_IN_METERS;
			_geoWatcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(_geoWatcher_PositionChanged);
			_geoWatcher.Start();

			// Disable the lock screen.
			PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
		}

		/// <summary>
		/// Occurs when the location service detects a change in position.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void _geoWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
		{
			if (_me == null)
			{
				// Is the first time we get our location so center the map on me.
				ViewModel.Center = e.Position.Location;

				// Drop a pushpin representing me.
				_me = new Pushpin();
				_me.Template = Resources["MePushpinTemplate"] as ControlTemplate;
				BingMap.Children.Add(_me);
			}

			// Update my location and the distance to the pin.
			_me.Location = e.Position.Location;
			ViewModel.Me = e.Position.Location;
			ViewModel.UpdateDistance();
		}

		/// <summary>
		/// Occurs when the user taps on the map.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BingMap_Tap(object sender, GestureEventArgs e)
		{
			if (_pin == null)
			{
				// Drop a pushpin representing the pin.
				_pin = new Pushpin();
				_pin.Template = Resources["PinPushpinTemplate"] as ControlTemplate;
				BingMap.Children.Add(_pin);
			}

			// Determine the coordinates of the point that was tapped.
			var point = e.GetPosition(BingMap);
			var newPinLocation = BingMap.ViewportPointToLocation(point);

			// Update pin's location and the distance to me.
			_pin.Location = newPinLocation;
			ViewModel.Pin = newPinLocation;
			ViewModel.UpdateDistance();
		}

		/// <summary>
		/// Occurs when the user clicks the "me" button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ApplicationBarCenterOnMeButton_Click(object sender, EventArgs e)
		{
			ViewModel.CenterOnMe();
		}

		/// <summary>
		/// Occurs when the user clicks the "pin" button.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ApplicationBarCenterOnPinButton_Click(object sender, EventArgs e)
		{
			ViewModel.CenterOnPin();
		}
	}
}