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

#import "MapViewController.h"

@implementation MapViewController

#pragma mark - Helper methods

//Convert degrees to radians which are returned
- (double)degreesToRadians:(double)degrees {
	return degrees * M_PI / 180;
}

//Calculate the distance between two points and return in miles
- (double)distanceBetweenPoints:(CLLocationCoordinate2D)pointA pointB:(CLLocationCoordinate2D)pointB {
	/*
	double radius = 3958.75587;
	double latDiff = fabs(pointA.latitude - pointB.latitude);
	double lonDiff = fabs(pointA.longitude - pointB.longitude);
	double radLatDiff = [self degreesToRadians:latDiff];
	double radLonDiff = [self degreesToRadians:lonDiff];
	double aRadLat = [self degreesToRadians:pointA.latitude];
	double bRadLat = [self degreesToRadians:pointB.latitude];
	double a = sin(radLatDiff / 2) * sin(radLatDiff / 2) + cos(aRadLat) * cos(bRadLat) * sin(radLonDiff / 2) * sin(radLonDiff / 2);
	double c = 2 * atan2(sqrt(a), sqrt(1 - a));
	return radius * c;
	 */
	
	CLLocation *userLoc = [[CLLocation alloc] initWithLatitude:pointA.latitude longitude:pointA.longitude ];
	CLLocation *pointLoc = [[CLLocation alloc] initWithLatitude:pointB.latitude longitude:pointB.longitude ];
	double distMiles = ([userLoc distanceFromLocation:pointLoc] / 1000) * 0.6214; //distanceFromLocation returns meters
	[userLoc release];
	[pointLoc release];
	
	return distMiles;
}

//Update the label on the view with the current distance between the points, also display an alert if they are close enough
- (void)updateLabel {
	double distance = [self distanceBetweenPoints:_mapView.userLocation.coordinate pointB:_pointAnnotation.coordinate];
	_label.text = [NSString stringWithFormat:@"Distance: %1.2f miles",distance];
	if (distance < kDistanceBetween) {	//check the distance
		UIAlertView *alertView = [[UIAlertView alloc] initWithTitle:@"You Have Arrived" message:@"Your location is on your pin." delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
		[alertView show];
		[alertView release];
	}
}

#pragma mark - UIGestureRecognizer methods

//Add gesture capture code to the map view
- (void)addGestureRecognizer {
	UILongPressGestureRecognizer *longPressGestureRecognizer = [[UILongPressGestureRecognizer alloc] initWithTarget:self action:@selector(longPressWithGestureRecognizer:)];
	longPressGestureRecognizer.minimumPressDuration = kGestureDuration;
	[_mapView addGestureRecognizer:longPressGestureRecognizer];
	[longPressGestureRecognizer release];
}

//Handle an incoming gesture
- (void)longPressWithGestureRecognizer:(UIGestureRecognizer *)gestureRecognizer {
	if (gestureRecognizer.state != UIGestureRecognizerStateBegan) {
		return;
	}
	
	[_mapView removeAnnotation:_pointAnnotation];	//remove previous pin
	[_pointAnnotation release];

	CGPoint point = [gestureRecognizer locationInView:_mapView];
	CLLocationCoordinate2D locationCoordinate = [_mapView convertPoint:point toCoordinateFromView:_mapView];
	_pointAnnotation = [[MKPointAnnotation alloc] init];
	_pointAnnotation.coordinate = locationCoordinate;
	[_mapView addAnnotation:_pointAnnotation];

	if (_mapView.userLocation != nil) {	//if we have a user, let's update the label
		[self updateLabel];
	}
}

#pragma mark - MKMapViewDelegate methods

//Called whenever the user's location is updated
- (void)mapView:(MKMapView *)mapView didUpdateUserLocation:(MKUserLocation *)userLocation {
	if (_pointAnnotation != nil) {	//if we have a pin, let's update the label
		[self updateLabel];
	}
}

#pragma mark - IBAction methods

//Center the view on the user's location
- (IBAction)buttonPressedCenterOnMe:(id)sender {
	if (_mapView.userLocation != nil) {
		[_mapView setCenterCoordinate:_mapView.userLocation.coordinate];
	}
}

//Center the view on the pin's location
- (IBAction)buttonPressedCenterOnPin:(id)sender {
	if (_pointAnnotation != nil) {
		[_mapView setCenterCoordinate:_pointAnnotation.coordinate];
	}
}

#pragma mark - UIViewController lifecycle methods

- (void)viewDidLoad {
	[super viewDidLoad];
	_mapView.showsUserLocation = YES;
	[self addGestureRecognizer];
}

- (void)dealloc {
	[_label release];
	[_mapView release];
	[_pointAnnotation release];
	[super dealloc];
}

@end
