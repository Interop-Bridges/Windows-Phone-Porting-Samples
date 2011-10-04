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

import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteDatabase.CursorFactory;
import android.database.sqlite.SQLiteOpenHelper;

/**
 * SQLite Database handler. Extends SQLiteOpenHelper and handles the database
 * creation, potential upgrades, etc
 * 
 */
public class GMSQLOpenHelper extends SQLiteOpenHelper {
	// Database Name and version constants
	public static final String DBNAME = "GroupMessage";
	public static final int DBVERSION = 1;

	// helper method for easy DB opening.
	public GMSQLOpenHelper(Context c) {
		super(c, DBNAME, null, DBVERSION);
	}

	public GMSQLOpenHelper(Context context, String name, CursorFactory factory,
			int version) {
		super(context, DBNAME, factory, DBVERSION);
	}

	// DB Initial Creation
	@Override
	public void onCreate(SQLiteDatabase db) {
		try {
			db.execSQL(MessageGroup.getTableCreationSQL());
			db.execSQL(GroupContact.getTableCreationSQL());
		} catch (Exception e) {
			// Your app should handle fail cases when they arise.
			e.printStackTrace();
		}
	}

	@Override
	public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
		// version #1 requires no upgrade...
	}

}
