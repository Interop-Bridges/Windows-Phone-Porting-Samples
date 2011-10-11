	//
//  iPhonePushReceiverViewController.m
//  iPhonePushReceiver
//
//  Created by Vivek Nirkhe on 4/18/11.
//  Copyright 2011 RevuBooks. All rights reserved.
//

#import "iPhonePushReceiverViewController.h"
#import "Wrapper.h"
#import "DDXML.h"

@implementation iPhonePushReceiverViewController



/*
// The designated initializer. Override to perform setup that is required before the view is loaded.
- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil {
    if ((self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil])) {
        // Custom initialization
    }
    return self;
}
*/



// Implement loadView to create a view hierarchy programmatically, without using a nib.
/*
- (void)loadView {

}
*/


// Implement viewDidLoad to do additional setup after loading the view, typically from a nib.
- (void)viewDidLoad {
    
    
    [super viewDidLoad];
    

}

-(void) SubscribeToNotification:(NSString *)sub
{
    NSString *deviceSubUrlStr = [NSString stringWithFormat:@"%@/push.svc/sub/add/%@/%@",PUSHSERVICE_URL, sub,  deviceId];    
    NSLog(@"subscription PushNotification: URL=%@",deviceSubUrlStr);    
    NSURL *url = [NSURL URLWithString:deviceSubUrlStr];
    NSDictionary *parameters = NULL;
    [engine sendRequestTo:url usingVerb:@"POST" withParameters:parameters];   
    currentCmd = @"addsub";
}

-(void) UnSubscribeToNotification:(NSString *)sub
{
    NSString *deviceSubUrlStr = [NSString stringWithFormat:@"%@/push.svc/sub/delete/%@/%@",PUSHSERVICE_URL, sub,  deviceId];    
    NSLog(@"subscription PushNotification: URL=%@",deviceSubUrlStr);    
    NSURL *url = [NSURL URLWithString:deviceSubUrlStr];
    NSDictionary *parameters = NULL;
    [engine sendRequestTo:url usingVerb:@"POST" withParameters:parameters];   
    //currentCmd = @"deletesub";
}


- (IBAction) flip: (id) sender {
    UISwitch *onoff = (UISwitch *) sender;
    int tag = [onoff tag];
    NSString *subName =  (NSString *)[subscriptions objectAtIndex:tag];
    if (onoff.on) {        
        NSLog(@"selected %@",subName);
        [self SubscribeToNotification:subName];
        
    } else 
    {
        NSLog(@"unselected %@",subName);
        [self UnSubscribeToNotification:subName];
    }
}

- (void) addSegmentedButton:(NSString *)title description:(NSString *) description count:(int) count isSubscribed:(BOOL)isSubscribed
{

    CGRect frame = CGRectMake(15, 90 + count*40, 100, 30);
    
    UILabel *lbl = [[UILabel alloc] initWithFrame:frame];
    [lbl setBackgroundColor:[UIColor clearColor]];
    [lbl setText:title];
    [self.view addSubview:lbl];

    frame = CGRectMake(130, 90  + count*40, 100, 30);
    UISwitch *onoff = [[UISwitch alloc] initWithFrame: frame];
    [onoff addTarget: self action: @selector(flip:) forControlEvents: UIControlEventValueChanged];
    [onoff setOn:isSubscribed];
    [onoff setTag:count];
    [self.view addSubview: onoff];
    [onoff release];
}

/*
// Override to allow orientations other than the default portrait orientation.
- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    // Return YES for supported orientations
    return (interfaceOrientation == UIInterfaceOrientationPortrait);
}
*/

- (void)didReceiveMemoryWarning {
	// Releases the view if it doesn't have a superview.
    [super didReceiveMemoryWarning];
	
	// Release any cached data, images, etc that aren't in use.
}

/*
- (void)viewDidUnload {
	// Release any retained subviews of the main view.
	// e.g. self.myOutlet = nil;
}
*/

#pragma mark -
#pragma mark WrapperDelegate methods

- (void)wrapper:(Wrapper *)wrapper didRetrieveData:(NSData *)data
{
    NSString *text = [engine responseAsText];
    NSLog(@"output is %@", text);
    if (text != nil)
    {
        //output.text = text;
        if ([currentCmd isEqualToString:@"subs"])
        {
            DDXMLDocument *ddDoc = [[[DDXMLDocument alloc] initWithXMLString:text options:0 error:NULL] autorelease];
            NSArray *ddChildren = [[ddDoc rootElement] children];
            subscriptions = [[NSMutableArray alloc] initWithCapacity:[ddChildren count]];
            for (int j = 0 ;j < [ddChildren count]; j++) {
                DDXMLElement *node = (DDXMLElement *)[ddChildren objectAtIndex:j];
                
                NSString *temp;
                if([[node elementsForName:@"Name"] count] > 0)
                    temp = [[[node elementsForName:@"Name"] objectAtIndex:0] stringValue]  ;
                
                
                
                DDXMLElement *nameNode = [[node elementsForName:@"Name"] objectAtIndex:0];
                DDXMLElement *descriptionNode = [[node elementsForName:@"Description"] objectAtIndex:0];
                DDXMLElement *isSubscribedNode = [[node elementsForName:@"IsSubscribed"] objectAtIndex:0];
                NSLog(@"Name is %@", [nameNode stringValue]);
                NSLog(@"Description is %@", [descriptionNode stringValue]);
                NSLog(@"IsSubscribed %@", [isSubscribedNode stringValue]);                
                BOOL isSubscribed = [[isSubscribedNode stringValue] isEqualToString:@"true"];
                [subscriptions insertObject:[nameNode stringValue] atIndex:j];	
                [self addSegmentedButton:[nameNode stringValue] description:[descriptionNode stringValue] count:j isSubscribed:isSubscribed];
            }                    

        }
    }
    NSLog(@"subs:%@",text);
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
    if (statusCode != 400) {
            UIAlertView *alert = [[UIAlertView alloc] initWithTitle:@"Status code not OK!" 
                                                    message:[NSString stringWithFormat:@"Status code not OK: %d!", statusCode]
                                                   delegate:self 
                                          cancelButtonTitle:@"OK" 
                                          otherButtonTitles:nil];
            [alert show];
            [alert release];
     }
}



#pragma mark -
#pragma mark AppLogic methods

-(void) setDeviceId:(NSString *)tokenStr
{
    deviceId   = [NSString stringWithString:tokenStr];
    //deviceId = [NSString stringWithString:@"8a7d5149f54f34ae264e433c3fcc5452da6c87225ac4bd701050c86629f34e0b"];
    if (engine == nil)
    {
        engine = [[Wrapper alloc] init];
        engine.delegate = self;
    }
    
    NSString *urlStr = [NSString stringWithFormat:@"%@/push.svc/subs/%@",PUSHSERVICE_URL, deviceId]; 
    NSLog(@"PushNotification: URL=%@",urlStr);    
    NSURL *url = [NSURL URLWithString:urlStr];
    NSDictionary *parameters = NULL;
    [engine sendRequestTo:url usingVerb:@"GET" withParameters:parameters];   
    currentCmd = @"subs";
}


#pragma mark -
#pragma mark MemoryManagement methods

- (void)dealloc {
    //[engine release];
    engine = nil;

    [super dealloc];
}

@end
