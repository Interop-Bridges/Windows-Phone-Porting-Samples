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

package com.microsoft.android.example.groupmessage.data;

import android.content.ContentValues;

public class GroupContact
	{
	// Unique identifier
	private int _id;
	
	// Contact display name
	private String _name;
	// phone number
	private String _phone;
	// contact ID from the contacts intent
	private int _contactId;
	// group Id for the group/contact relationship
	private int _groupId;

	// Static database creation SQL (in string form)
	public static String getTableCreationSQL()
		{
		return "create table myContacts (_id INTEGER PRIMARY KEY, groupId INTEGER, name TEXT, phone TEXT, contactId INTEGER);";
		}

	// standard constructor
	public GroupContact(int id, int groupId, String name, String phone, int contactId)
		{
		_id = id;
		_name = name;
		_contactId = contactId;
		_groupId = groupId;
		_phone = phone;
		}

	// constructor for adding a new contact to the DB (ie, no id set yet) 
	public GroupContact(int groupId, String name, String phone, int contactId)
		{
		_name = name;
		_contactId = contactId;
		_groupId = groupId;
		_phone = phone;
		}

	// To String method (required in order to use an ArrayAdapter).
	public String toString()
		{
		return _name;
		}

	public void setId(int id)
		{
		_id = id;
		}

	public int getId()
		{
		return _id;
		}

	public void setName(String name)
		{
		_name = name;
		}

	public String getName()
		{
		return _name;
		}

	public void setContactId(int contactId)
		{
		_contactId = contactId;
		}

	public int getContactId()
		{
		return _contactId;
		}

	public void setGroupId(int groupId)
		{
		_groupId = groupId;
		}

	public int getGroupId()
		{
		return _groupId;
		}

	public void setPhone(String phone)
		{
		_phone = phone;
		}

	public String getPhone()
		{
		return _phone;
		}

	// Method of convenience for grabbing the ContentValues
	public ContentValues getContentValues()
		{
		ContentValues returnVal = new ContentValues();
		returnVal.put("phone", getPhone());
		returnVal.put("name", getName());
		returnVal.put("groupId", getGroupId());

		return returnVal;
		}
	}
