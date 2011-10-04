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

import java.util.ArrayList;

import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteException;

/*
 * Convenience class for talking to the database.
 * 
 * Usage:  
 * 1) open the Database with your Context (an activity works well).
 * 2) grab the information you need (or create a new method here that suits your needs)
 * 3) close the database when you're done (don't leave it open!)
 * 
 */
public class GMDBController {
	private static SQLiteDatabase mSQLDB;
	private static GMDBController _dbData;
	private static GMSQLOpenHelper mDBHelper;

	// Make sure to only open the database one time
	public static GMDBController openWithContext(Context context) {
		if (_dbData == null) {
			_dbData = new GMDBController(context);
		}
		mSQLDB = mDBHelper.getWritableDatabase();
		return _dbData;
	}

	private GMDBController(Context context) {
		_dbData = this;
		mDBHelper = new GMSQLOpenHelper(context);
	}

	public void close() {
		mDBHelper.close();
	}

	/*
	 * Get all the Groups from the database.
	 */
	public ArrayList<MessageGroup> getMessageGroups() {
		Cursor myCursor = null;
		ArrayList<MessageGroup> returnValues = new ArrayList<MessageGroup>();
		try {
			myCursor = mSQLDB.query(true, "myGroups", null, null, null, null,
					null, null, null);

			if (myCursor != null) {
				myCursor.moveToFirst();
				int numResults = myCursor.getCount();

				for (int i = 0; i < numResults; i++) {
					// alternatively, you could use myCursor.moveToNext() in a
					// while loop.
					myCursor.moveToPosition(i);

					// get the integer id from the first column, the group name
					// from the second.
					MessageGroup tmp = new MessageGroup(myCursor.getInt(0),
							myCursor.getString(1));
					returnValues.add(tmp);
				}

			}
		} catch (SQLiteException e) {
			e.printStackTrace();
		} finally {
			if (myCursor != null) {
				myCursor.close();
			}
		}
		return returnValues;
	}

	/*
	 * Grab the contacts given a groupId
	 */
	public ArrayList<GroupContact> getGroupContacts(int currentGroup) {
		Cursor myCursor = null;
		ArrayList<GroupContact> returnValues = new ArrayList<GroupContact>();
		try {
			myCursor = mSQLDB.query(true, "myContacts", null, "groupId="
					+ currentGroup, null, null, null, null, null);

			if (myCursor != null) {
				myCursor.moveToFirst();
				int numResults = myCursor.getCount();

				for (int i = 0; i < numResults; i++) {
					// get the integer id from the first column, the group name
					// from the second, etc. See GroupContact for details
					myCursor.moveToPosition(i);
					// construct the contact from the cursor (this should be
					// handled with constants from your Database).
					GroupContact tmp = new GroupContact(myCursor.getInt(0),
							myCursor.getInt(1), myCursor.getString(2),
							myCursor.getString(3), myCursor.getInt(4));
					returnValues.add(tmp);
				}
			}
		} catch (SQLiteException e) {
			e.printStackTrace();
		} finally {
			// make sure to close each cursor you open.
			if (myCursor != null) {
				myCursor.close();
			}
		}
		return returnValues;
	}

	/**
	 * @param currentGroup
	 * @param groupContact
	 * @return
	 * 
	 *         Add or replace contacts given a group Id and a contact
	 */
	public long addContactToGroup(int currentGroup, GroupContact groupContact) {
		return mSQLDB.replace("myContacts", null,
				groupContact.getContentValues());
	}
	/**
	 * Create a new Group in the database.
	 * 
	 * @return
	 */
	public long createContactGroup() {
		MessageGroup tmp = new MessageGroup();
		return mSQLDB.insert("myGroups", null, tmp.getContentValues());
	}

	// Update the group name given a group
	public void updateGroupName(int currentGroup, String name) {
		MessageGroup tmp = new MessageGroup(currentGroup, name);
		mSQLDB.update("myGroups", tmp.getContentValues(),
				"_id=" + currentGroup, null);
	}

	// delete the group and all contact relationships to the group
	public void removeMessageGroup(int id) {
		mSQLDB.delete("myGroups", "_id=" + id, null);
		mSQLDB.delete("myContacts", "groupId=" + id, null);
	}

	public void removeContactFromGroup(int currentGroup,
			GroupContact groupContact) {
		mSQLDB.delete("myContacts", "groupId=" + currentGroup + " and _id="
				+ groupContact.getId(), null);
	}

}
