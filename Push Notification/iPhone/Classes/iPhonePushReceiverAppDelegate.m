//
//  iPhonePushReceiverAppDelegate.m
//  iPhonePushReceiver
//
//  Created by Vivek Nirkhe on 4/18/11.
//  Copyright 2011 RevuBooks. All rights reserved.
//

#import "iPhonePushReceiverAppDelegate.h"
#import "iPhonePushReceiverViewController.h"
#import "Wrapper.h"

@implementation iPhonePushReceiverAppDelegate

@synthesize window;
@synthesize viewController;

#pragma mark -
#pragma mark Device Token

- (void)application:(UIApplication *)app didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken { 
	
    NSLog(@"PushNotification: Device Token=%@",deviceToken);
    if (engine == nil)
    {
        engine = [[Wrapper alloc] init];
        engine.delegate = self;
    }
    
    NSString *tStr = [[[deviceToken description] 
                      stringByTrimmingCharactersInSet:[NSCharacterSet characterSetWithCharactersInString:@"<>"]] 
                     stringByReplacingOccurrencesOfString:@" " withString:@""];    
    tokenAsString = [[NSString alloc] initWithString:tStr];
    NSString *urlStr = [NSString stringWithFormat:@"%@/iOSDevice.svc/register/%@",PUSHSERVICE_URL, tokenAsString]; 
    NSLog(@"PushNotification: URL=%@",urlStr);    
    NSURL *url = [NSURL URLWithString:urlStr];
    NSDictionary *parameters = NULL;
    [engine sendRequestTo:url usingVerb:@"POST" withParameters:parameters];    
    
	
}

- (void)application:(UIApplication *)app didFailToRegisterForRemoteNotificationsWithError:(NSError *)err { 
	
    NSLog(@"PushNotification: Error: %@", err);    
	
}



#pragma mark -
#pragma mark Application lifecycle

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {    
    
    // Override point for customization after application launch.

    // Add the view controller's view to the window and display.
    [window addSubview:viewController.view];
    [window makeKeyAndVisible];
	
    NSLog(@"Registering for push notifications...");    
    [[UIApplication sharedApplication] 
	 registerForRemoteNotificationTypes:
	 (UIRemoteNotificationTypeAlert | 
	  UIRemoteNotificationTypeBadge | 
	  UIRemoteNotificationTypeSound)];

    return YES;
}

- (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo
{
    NSString *alertMsg;
    NSString *badge;
    NSString *sound;
    
    if( [[userInfo objectForKey:@"aps"] objectForKey:@"alert"] != NULL)
    {
        alertMsg = [[userInfo objectForKey:@"aps"] objectForKey:@"alert"]; 
    }
    else
    {    alertMsg = @"{no alert message in dictionary}";
    }
    
    if( [[userInfo objectForKey:@"aps"] objectForKey:@"badge"] != NULL)
    {
        badge = [[userInfo objectForKey:@"aps"] objectForKey:@"badge"]; 
    }
    else
    {    badge = @"{no badge number in dictionary}";
    }
    
    if( [[userInfo objectForKey:@"aps"] objectForKey:@"sound"] != NULL)
    {
        sound = [[userInfo objectForKey:@"aps"] objectForKey:@"sound"]; 
    }
    else
    {    sound = @"{no sound in dictionary}";
    }
    
    //AudioServicesPlaySystemSound (kSystemSoundID_Vibrate); 
    
    NSString* alert_msg = [NSString stringWithFormat:@"APNS message '%@' was just received.", alertMsg];
    
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"alert received" 
                                                    message:alert_msg 
                                                   delegate:nil 
                                          cancelButtonTitle:@"OK" 
                                          otherButtonTitles:nil];
    [alert show];
    [alert release];
    
}

- (void)applicationWillResignActive:(UIApplication *)application {
    /*
     Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
     Use this method to pause ongoing tasks, disable timers, and throttle down OpenGL ES frame rates. Games should use this method to pause the game.
     */
}


- (void)applicationDidEnterBackground:(UIApplication *)application {
    /*
     Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later. 
     If your application supports background execution, called instead of applicationWillTerminate: when the user quits.
     */
}


- (void)applicationWillEnterForeground:(UIApplication *)application {
    /*
     Called as part of  transition from the background to the inactive state: here you can undo many of the changes made on entering the background.
     */
}


- (void)applicationDidBecomeActive:(UIApplication *)application {
    /*
     Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
     */
}


- (void)applicationWillTerminate:(UIApplication *)application {
    /*
     Called when the application is about to terminate.
     See also applicationDidEnterBackground:.
     */
}
#pragma mark -
#pragma mark WrapperDelegate methods

- (void)wrapper:(Wrapper *)wrapper didRetrieveData:(NSData *)data
{
    NSString *text = [engine responseAsText];
    if (text != nil)
    {
        //output.text = text;
        [viewController setDeviceId:tokenAsString];        
        
    }
    [UIApplication sharedApplication].networkActivityIndicatorVisible = NO;
}

- (void)wrapperHasBadCredentials:(Wrapper *)wrapper
{
    [UIApplication sharedApplication].networkActivityIndicatorVisible = NO;
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Bad credentials!" 
                                                    message:@"Bad credentials!"  
                                                   delegate:self 
                                          cancelButtonTitle:@"OK" 
                                          otherButtonTitles:nil];
    [alert show];
    [alert release];
}

- (void)wrapper:(Wrapper *)wrapper didCreateResourceAtURL:(NSString *)url
{
    [UIApplication sharedApplication].networkActivityIndicatorVisible = NO;
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Resource created!" 
                                                    message:[NSString stringWithFormat:@"Resource created at %@!", url]  
                                                   delegate:self 
                                          cancelButtonTitle:@"OK" 
                                          otherButtonTitles:nil];
    [alert show];
    [alert release];
}

- (void)wrapper:(Wrapper *)wrapper didFailWithError:(NSError *)error
{
    [UIApplication sharedApplication].networkActivityIndicatorVisible = NO;
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Error!" 
                                                    message:[NSString stringWithFormat:@"Error code: %d!", [error code]]  
                                                   delegate:self 
                                          cancelButtonTitle:@"OK" 
                                          otherButtonTitles:nil];
    [alert show];
    [alert release];
}

- (void)wrapper:(Wrapper *)wrapper didReceiveStatusCode:(int)statusCode
{
    [UIApplication sharedApplication].networkActivityIndicatorVisible = NO;
    UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Status code not OK!" 
                                                    message:[NSString stringWithFormat:@"Status code not OK: %d!", statusCode]
                                                   delegate:self 
                                          cancelButtonTitle:@"OK" 
                                          otherButtonTitles:nil];
    [alert show];
    [alert release];
}

#pragma mark -
#pragma mark Memory management

- (void)applicationDidReceiveMemoryWarning:(UIApplication *)application {
    /*
     Free up as much memory as possible by purging cached data objects that can be recreated (or reloaded from disk) later.
     */
}


- (void)dealloc {
    [viewController release];
    [window release];
    [tokenAsString release];
    [super dealloc];
}


@end
