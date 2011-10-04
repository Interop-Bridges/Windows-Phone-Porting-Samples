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

package com.microsoft.android.example.geolocation;

import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.List;

import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.graphics.drawable.Drawable;
import android.location.Criteria;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.widget.TextView;
import android.widget.Toast;

import com.google.android.maps.GeoPoint;
import com.google.android.maps.ItemizedOverlay;
import com.google.android.maps.MapActivity;
import com.google.android.maps.MapController;
import com.google.android.maps.MapView;
import com.google.android.maps.Overlay;
import com.google.android.maps.OverlayItem;

public class MainActivity extends MapActivity implements LocationListener {

	// final used to calculate distance
	private static final double METERS_PER_MILE = 1609.344;
	// the radius, in meters, for the proximity alarm
	private static final float PROXIMITY_RADIUS = 60.0f;
	// time interval in MS for location updates
	private static final long LOCATION_UPDATE_INTERVAL = 30 * 1000;
	// tag used for intent for BroadcastReceiver
	private static final String INTENT_TAG = "com.microsoft.example.geolocation.intent";

	private TextView mDistanceText; 	// TextView on the layout, used for distance
	private MapView mMapView;
	private LocationManager mLocationManager;
	private MapController mMapController;
	private GeoPoint mUserGeoPoint; 	// user's current location
	private GeoPoint mSelectedGeoPoint; // user's selected location
	
	// pin graphics for map
	private Drawable mUserPin;
	private Drawable mSelectedPin;

	private PendingIntent mProximityIntent;
	private ProximityReceiver mProximityReceiver;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		// NOTE : if user's debug api key isn't put in /res/layout/main.xml then
		// maps will not have tiles
		setContentView(R.layout.main);

		// get the drawable resources for easy re-use
		mUserPin = getResources().getDrawable(R.drawable.my_location_pin);
		mSelectedPin = getResources().getDrawable(R.drawable.map_pin);

		// get the views we need to interact with programmatically
		mDistanceText = (TextView) findViewById(R.id.distance_text);
		mMapView = (MapView) findViewById(R.id.map_view);
		mMapView.setBuiltInZoomControls(true);

		mLocationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);

		// determine the best provider - mock gps on emulator
		Criteria criteria = new Criteria();
		String bestProvider = mLocationManager.getBestProvider(criteria, true);

		Location location = mLocationManager.getLastKnownLocation(bestProvider);

		// no location? user probably forgot to set gps via DDMS or is in airplane mode (etc)
		if (location == null) {
			// This will work much better with a mock gps 
			//in eclipse : Window -> Open Perspective -> DDMS -> emulator control -> location controls -> manual
			
			//use Redmond, WA as the default location.
			mUserGeoPoint = new GeoPoint((int) (47.6741667 * 1E6), (int) (-122.1202778 * 1E6));
			
			Toast.makeText(this, "This will work much better with mock coordinates. Read the README!", Toast.LENGTH_LONG).show();
		} 
		else {
			//user has gps enabled, request periodic updates
			mLocationManager.requestLocationUpdates(bestProvider, LOCATION_UPDATE_INTERVAL, PROXIMITY_RADIUS * .8f, this);

			// get the user geo point
			mUserGeoPoint = new GeoPoint((int) (location.getLatitude() * 1E6),(int) (location.getLongitude() * 1E6));
		}

		// get controller and set default zoom
		mMapController = mMapView.getController();
		mMapController.setZoom(14);

		// this will be set once the user selects a location
		mSelectedGeoPoint = null;
		mProximityReceiver = new ProximityReceiver();

		mMapView.getOverlays().add(getUserOverlay());
		mMapController.animateTo(mUserGeoPoint);
	}

	@Override
	protected void onStop() {
		super.onStop();

		// turn off location updates
		mLocationManager.removeUpdates(this);

		// unregister proximity receiver if there is an intent
		if (mProximityIntent != null) {
			unregisterReceiver(mProximityReceiver);
		}
	}

	/**
	 * Adds a proximity alert to the given latitude/longitude. It will go off if
	 * the user's location is within PROXIMITY_RADIUS meters.
	 */
	private void addProximityAlert(double latitude, double longitude) {
		// If there is an intent, we need to unregister it before adding
		// another- otherwise they aggregate.
		if (mProximityIntent != null) {
			unregisterReceiver(mProximityReceiver);
		}

		// create the ProximityIntent with an intent using the INTENT_TAG
		Intent intent = new Intent(INTENT_TAG);
		mProximityIntent = PendingIntent.getBroadcast(MainActivity.this, 0, intent, 0);
		mLocationManager.addProximityAlert(latitude, longitude, PROXIMITY_RADIUS, -1, mProximityIntent);

		// create the filter and register the ProximityReceiver
		IntentFilter filter = new IntentFilter(INTENT_TAG);
		registerReceiver(mProximityReceiver, filter);
	}

	/**
	 * Update the pins on the map.
	 */
	private void updateMapOverlays() {
		List<Overlay> mapOverlays = mMapView.getOverlays();
		mapOverlays.clear();
		mapOverlays.add(getUserOverlay());
		if (mSelectedGeoPoint != null) {
			mapOverlays.add(getSelectedOverlay());
		}
		mMapView.invalidate();
		updateDistanceText();
	}

	/**
	 * Returns the user overlay (pin).
	 */
	private MapOverlay getUserOverlay() {
		MapOverlay myOverlay = new MapOverlay(mUserPin);
		OverlayItem overlayItem = new OverlayItem(mUserGeoPoint, "", "");
		myOverlay.addOverlay(overlayItem);
		return myOverlay;
	}

	/**
	 * Returns the selected overlay (pin). Should only be called if
	 * mSelectedGeoPoint is NOT null.
	 */
	private MapOverlay getSelectedOverlay() {
		MapOverlay selectedOverlay = new MapOverlay(mSelectedPin);
		OverlayItem overlayItem = new OverlayItem(mSelectedGeoPoint, "", "");
		selectedOverlay.addOverlay(overlayItem);
		return selectedOverlay;
	}

	/**
	 * Updates the distance text field with the new distance. Called when there
	 * is an update in user location or selected location.
	 */
	private void updateDistanceText() {
		// make sure we have both points
		if (mUserGeoPoint != null && mSelectedGeoPoint != null) {
			double distance = calculateDistanceInMiles();

			BigDecimal bd = new BigDecimal(Double.toString(distance));
			bd = bd.setScale(2, BigDecimal.ROUND_HALF_UP);

			mDistanceText.setText("Distance: " + bd.toString() + " miles");
		}
	}

	/**
	 * Calculates distance in miles between the user location and the selected
	 * location.
	 * 
	 * @return The distance delta in miles.
	 */
	private double calculateDistanceInMiles() {
		// create Location objects with lat/longs and determine distance.
		Location locA = new Location("");
		locA.setLatitude(mUserGeoPoint.getLatitudeE6() / 1E6f);
		locA.setLongitude(mUserGeoPoint.getLongitudeE6() / 1E6f);
		Location locB = new Location("");
		locB.setLatitude(mSelectedGeoPoint.getLatitudeE6() / 1E6f);
		locB.setLongitude(mSelectedGeoPoint.getLongitudeE6() / 1E6f);

		return locA.distanceTo(locB) / METERS_PER_MILE;
	}

	@Override
	protected boolean isRouteDisplayed() {
		// must be overridden for extending MapView; no routes shown
		return false;
	}

	// part of LocationListener interface, called when user location is changed
	public void onLocationChanged(Location location) {
		// construct the new user location GeoPoint
		mUserGeoPoint = new GeoPoint((int) (location.getLatitude() * 1E6),
				(int) (location.getLongitude() * 1E6));

		updateMapOverlays();
	}

	// unneeded method from LocationListener interface
	public void onProviderDisabled(String provider) {
	}

	// unneeded method from LocationListener interface
	public void onProviderEnabled(String provider) {
	}

	// unneeded method from LocationListener interface
	public void onStatusChanged(String provider, int status, Bundle extras) {
	}

	private class MapOverlay extends ItemizedOverlay<OverlayItem> {
		// the length in MS to determine how long the user must hold to place a pin
		private static final long PRESS_THRESHOLD_IN_MS = 200L;

		// the x, y maximum delta threshold, to prevent needless pin dropping if user
		// is moving around on map
		private static final float COORDINATE_DELTA_THRESHOLD = 20.0f;

		// the start x,y for a 'down' touch event, used to calculate deltas
		private float mStartX = 0.0f;
		private float mStartY = 0.0f;

		// the stored overlays
		private ArrayList<OverlayItem> mOverlays = new ArrayList<OverlayItem>();

		public MapOverlay(Drawable defaultMarker) {
			super(boundCenterBottom(defaultMarker));
		}

		public void addOverlay(OverlayItem overlay) {
			mOverlays.add(overlay);
			populate();
		}

		@Override
		protected OverlayItem createItem(int i) {
			return mOverlays.get(i);
		}

		@Override
		public int size() {
			return mOverlays.size();
		}

		@Override
		public boolean onTouchEvent(MotionEvent event, MapView mapView) {
			// this function works by determining the time, x and y deltas of finger placement on the screen
			// between pushing down and pulling up. If the coordinate deltas are sufficiently small (as
			// determined by COORDINATE_DELTA_THRESHOLD) and the time is sufficiently long (as determined
			// by PRESS_THRESHOLD_IN_MS) then the user will drop a pin in that location.
			int action = event.getAction();

			if (action == MotionEvent.ACTION_DOWN) {
				// down pressed, record the coordinates
				mStartX = event.getRawX();
				mStartY = event.getRawY();
			} else if (action == MotionEvent.ACTION_UP
					&& (event.getEventTime() - event.getDownTime()) >= PRESS_THRESHOLD_IN_MS) {

				// user pulled up, get ending x and y position
				float x = event.getRawX();
				float y = event.getRawY();

				// is the coordinate delta within threshold?
				if (isWithinCoordinateThreshold(mStartX - x, mStartY - y)) {

					// get the GeoPoint of the new pin
					mSelectedGeoPoint = mapView.getProjection().fromPixels((int) x, (int) y);

					// a new pin is being dropped add a proximity alert for it
					addProximityAlert(mSelectedGeoPoint.getLatitudeE6() / 1E6f, mSelectedGeoPoint.getLongitudeE6() / 1E6f);
					
					// update overlays, change distance text, etc
					updateMapOverlays();
				}
			}
			return super.onTouchEvent(event, mapView);
		}

		/**
		 * Determines if the user's press delta is within acceptable means.
		 * 
		 * @param x
		 *            The x coordinate delta from the ACTION_DOWN and ACTION_UP
		 *            event.
		 * @param y
		 *            The y coordinate delta from the ACTION_DOWN and ACTION_UP
		 *            event.
		 * @return True if within the threshold, false otherwise.
		 */
		private boolean isWithinCoordinateThreshold(float x, float y) {
			x = Math.abs(x);
			y = Math.abs(y);

			return x <= COORDINATE_DELTA_THRESHOLD && y <= COORDINATE_DELTA_THRESHOLD;
		}
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// inflate the options menu
		MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.mapping_menu, menu);
		return true;
	}

	@Override
	public boolean onPrepareOptionsMenu(Menu menu) {
		// iterate over menu items, and hide "center on pin" if user hasn't
		// dropped one
		for (int i = 0; i < menu.size(); i++) {
			MenuItem item = menu.getItem(i);
			if (item.getItemId() == R.id.center_pin
					&& mSelectedGeoPoint == null) {
				item.setVisible(false);
			} else {
				item.setVisible(true);
			}
		}
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
			case R.id.center_me :
				// user selected to center on their location
				mMapController.animateTo(mUserGeoPoint);
				return true;
			case R.id.center_pin :
				// user selected to center on their selected location
				mMapController.animateTo(mSelectedGeoPoint);
				return true;
			default :
				return super.onOptionsItemSelected(item);
		}
	}
}