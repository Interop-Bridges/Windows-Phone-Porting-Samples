using System;
using System.ComponentModel;
using System.Device.Location;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls.Maps;

namespace GeoLocation
{
	/// <summary>
	/// View-model for MainPage.xaml
	/// </summary>
	public class MainPageViewModel : INotifyPropertyChanged
	{
		// NOTE: Use your real Bing Maps API key here.
		private const string BING_MAPS_API_KEY = "Art8SsThPg9HO5ZayBlhHc0dRsDKYNay8ZKy6Q_VL_3K6pPnl4sycwufLYSU-FWy";

		private const double METERS_PER_MILE = 0.000621371192;
		private const int THRESHOLD_FOR_ARRIVING_IN_METERS = 10;

		/// <summary>
		/// The coordinates of the center of the map.
		/// </summary>
		private GeoCoordinate _center;
		public GeoCoordinate Center
		{
			get { return _center; }
			set { _center = value; RaisePropertyChanged("Center"); }
		}

		/// <summary>
		/// The coordinates where I am located.
		/// </summary>
		private GeoCoordinate _me;
		public GeoCoordinate Me
		{
			get { return _me; }
			set { _me = value; RaisePropertyChanged("Me"); }
		}

		/// <summary>
		/// The coordinates where the pin is located.
		/// </summary>
		private GeoCoordinate _pin;
		public GeoCoordinate Pin
		{
			get { return _pin; }
			set { _pin = value; RaisePropertyChanged("Pin"); }
		}

		/// <summary>
		/// The zoom level of the map.
		/// </summary>
		private int _zoom;
		public int Zoom
		{
			get { return _zoom; }
			set { _zoom = value; RaisePropertyChanged("Zoom"); }
		}

		/// <summary>
		/// The distance (in miles) between Me and Pin.
		/// </summary>
		private double _distance;
		public double Distance
		{
			get { return _distance; }
			set { _distance = value; RaisePropertyChanged("Distance"); }
		}

		/// <summary>
		/// Bindable credentails provider for the Map control.
		/// </summary>
		private CredentialsProvider _credentialsProvider = new ApplicationIdCredentialsProvider(BING_MAPS_API_KEY);
		public CredentialsProvider CredentialsProvider
		{
			get { return _credentialsProvider; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public MainPageViewModel()
		{
			// Pioneer Courthouse Square in Portland, Oregon
			Center = new GeoCoordinate(45.51885, -122.679211);
			Zoom = 18;
		}

		/// <summary>
		/// Calculates and updates the distance between Me and Pin.
		/// Also notifies the user if they are close enough to be
		/// considered as having arrived at the Pin.
		/// </summary>
		public void UpdateDistance()
		{
			if (Me == null || Pin == null)
			{
				// short-circuit and don't perform the calculations if we don't have both points
				return;
			}

			var distanceInMeters = Me.GetDistanceTo(Pin);

			// convert meters to miles for display
			Distance = distanceInMeters * METERS_PER_MILE;

			// notify if we are close enough to have arrived
			if (distanceInMeters < THRESHOLD_FOR_ARRIVING_IN_METERS)
			{
				MessageBox.Show("Your current location is on your pin.", "You Have Arrived", MessageBoxButton.OK);
			}
		}

		/// <summary>
		/// Updates the center of the map to be Me.
		/// </summary>
		public void CenterOnMe()
		{
			if (Me != null)
			{
				Center = Me;
			}
		}

		/// <summary>
		/// Updates the center of the map to be Pin.
		/// </summary>
		public void CenterOnPin()
		{
			if (Pin != null)
			{
				Center = Pin;
			}
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}

		#endregion
	}
}
