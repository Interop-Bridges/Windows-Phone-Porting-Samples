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

#import "MainViewController.h"

@implementation MainViewController

- (void)viewDidLoad {
	[super viewDidLoad];
	//Setup the Ad banner view and add it to the view
	ADBannerView *bannerView = [[ADBannerView alloc] initWithFrame:CGRectMake(0, 460, 320, 50)];
	bannerView.currentContentSizeIdentifier = ADBannerContentSizeIdentifierPortrait;
	bannerView.delegate = self;
	[self.view addSubview:bannerView];
	[bannerView release];
}

#pragma mark - ADBannerViewDelegate methods

//Called when a banner view loads an Ad, so let's animate in on screen
- (void)bannerViewDidLoadAd:(ADBannerView *)banner {
	[UIView animateWithDuration:0.5f animations:^{
		banner.frame = CGRectOffset(banner.frame, 0, -50);
	}];
}

//Called when a banner view encounters an error, so let's animate the view off screen
- (void)bannerView:(ADBannerView *)banner didFailToReceiveAdWithError:(NSError *)error {
	[UIView animateWithDuration:0.5f animations:^{
		banner.frame = CGRectOffset(banner.frame, 0, 50);
	}];
}

@end
