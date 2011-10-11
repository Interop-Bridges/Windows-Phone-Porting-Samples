//
//  iPhonePushReceiverAppDelegate.h
//  iPhonePushReceiver
//
//  Created by Vivek Nirkhe on 4/18/11.
//  Copyright 2011 RevuBooks. All rights reserved.
//

#import <UIKit/UIKit.h>
@class iPhonePushReceiverViewController;
#import "WrapperDelegate.h"

@class Wrapper;

@interface iPhonePushReceiverAppDelegate : NSObject <UIApplicationDelegate,WrapperDelegate> {
    UIWindow *window;
    iPhonePushReceiverViewController *viewController;
    Wrapper *engine;
    NSString * tokenAsString;

}

@property (nonatomic, retain) IBOutlet UIWindow *window;
@property (nonatomic, retain) IBOutlet iPhonePushReceiverViewController *viewController;

@end

