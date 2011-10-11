//
//  iPhonePushReceiverViewController.h
//  iPhonePushReceiver
//
//  Created by Vivek Nirkhe on 4/18/11.
//  Copyright 2011 RevuBooks. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "WrapperDelegate.h"
#import <libxml/xmlreader.h>

@class Wrapper;

@interface iPhonePushReceiverViewController : UIViewController<WrapperDelegate> {
    Wrapper *engine;
    NSString *deviceId;
    NSString *currentCmd;
    NSMutableArray *subscriptions;
}


-(void) setDeviceId:(NSString*) tokenStr;

@end

