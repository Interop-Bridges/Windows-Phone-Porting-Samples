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

#import <AddressBookUI/AddressBookUI.h>

@class Group;

@interface GroupDetailViewController : UIViewController <ABPeoplePickerNavigationControllerDelegate, NSFetchedResultsControllerDelegate>
{
    UITextField *groupName;
}

@property (nonatomic, retain) NSManagedObjectContext *managedObjectContext;
@property (nonatomic, retain) NSFetchedResultsController *fetchedResultsController;
@property (nonatomic, retain) IBOutlet UITableView *tableView;

@property (nonatomic, retain) Group *group;

@property (nonatomic, assign) BOOL isNew;

@property (nonatomic, retain) IBOutlet UITextField *groupName;

- (id)initWithGroup:(Group *)inGroup;

@end
