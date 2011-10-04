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

/* This class is just for convenience.  All data handling comes from the database
 */
public class MessageGroup {
	private int _id;
	private String _name;

	// Schema for the Group
	public static String getTableCreationSQL() {
		return "create table myGroups (_id INTEGER PRIMARY KEY, name TEXT);";
	}

	public MessageGroup(int id, String name) {
		setId(id);
		_name = name;
	}

	public MessageGroup() {
		// Unique string for a new group
		_name = "New Group";
	}

	// Required for Array Adapter
	public String toString() {
		return _name;
	}

	public void setName(String name) {
		_name = name;
	}

	public String getName() {
		return _name;
	}

	public void setId(int id) {
		_id = id;
	}

	public int getId() {
		return _id;
	}

	// Database inserts require ContentValues.  
	// These are simply methods of convenience for creating those on the fly
	public ContentValues getContentValues() {
		ContentValues values = new ContentValues();
		values.put("name", getName());
		return values;
	}

	public ContentValues getAllContentValues() {
		ContentValues values = new ContentValues();
		values.put("name", getName());
		values.put("_id", getId());
		return values;
	}

}
