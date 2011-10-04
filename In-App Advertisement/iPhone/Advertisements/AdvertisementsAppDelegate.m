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

#import "AdvertisementsAppDelegate.h"
#import "MainViewController.h"

@implementation AdvertisementsAppDelegate


@synthesize window=_window;

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
	//Add the view controller to the main window
	MainViewController *mainViewController = [[MainViewController alloc] init];
	[self.window setRootViewController:mainViewController];
	[mainViewController release];
	[self.window makeKeyAndVisible];
    return YES;
}

- (void)dealloc {
	[_window release];
    [super dealloc];
}

@end
