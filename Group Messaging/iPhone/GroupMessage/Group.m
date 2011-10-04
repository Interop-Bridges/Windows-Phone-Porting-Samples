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

#import "Group.h"
#import "Member.h"

@implementation Group
@dynamic name;
@dynamic members;

- (NSString *)description
{
    NSInteger memberCount = [self.members count];
    return [NSString stringWithFormat:@"%@ (%d)", self.name, memberCount];
}

- (NSArray *)memberPhoneNumbers
{
    NSMutableArray *memberPhoneNumbers = [NSMutableArray array];
    for (Member *member in [self.members allObjects])
    {
        [memberPhoneNumbers addObject:member.phoneNumber];
    }
    return memberPhoneNumbers;
}

@end
