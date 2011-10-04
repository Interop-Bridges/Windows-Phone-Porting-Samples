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

package com.microsoft.android.example.groupmessage;

import java.util.ArrayList;

import android.app.ListActivity;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Bundle;
import android.provider.ContactsContract;
import android.view.ContextMenu;
import android.view.ContextMenu.ContextMenuInfo;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.microsoft.android.example.groupmessage.data.GMDBController;
import com.microsoft.android.example.groupmessage.data.GroupContact;

public class EditGroupActivity extends ListActivity implements OnClickListener {

	private static final int ADD_CONTACT = 101;
	private int _currentGroup = -1;
	private TextView _addContactButton;
	private Button _commitName;
	private EditText _groupName;

	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.Activity#onCreate(android.os.Bundle)
	 */
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.edit_group);

		// This is the "Add" button for an empty list. You can add via the menu
		// or via this button.
		_addContactButton = (TextView) findViewById(R.id.addContactButton);

		// This button is the "commit name change" button. Usually you would put
		// the edit functionality in
		// a separate view but this works as a quick demo.
		_commitName = (Button) findViewById(R.id.commit_name);

		// We implement onClickListener so we can pass in ourselves as the
		// onClickListener
		_commitName.setOnClickListener(this);
		_addContactButton.setOnClickListener(this);

		// this is the name field. We just need a handle for it to commit the
		// value to the DB and
		// set the field on the fly when we build the view.
		_groupName = (EditText) findViewById(R.id.groupNameField);

		// Grab the extras that we passed into the activity when we started it.
		Bundle extras = getIntent().getExtras();
		if (extras != null) {
			// Default value is set to -1. There are several ways to handle
			// this. (pass in a flag, etc)
			_currentGroup = extras.getInt("GROUP_ID", -1);
		}

		// Create a new group or load our existing group from the database.
		// Both of these actions are put onto a background task.
		if (_currentGroup == -1) {
			new CreateNewGroup().execute();
		} else {
			_groupName.setText(extras.getString("GROUP_NAME"));
			new GetContactsForGroup().execute();
		}

		// register context menus for our listview
		registerForContextMenu(getListView());

	}

	// Our implemented interface for View clicks
	public void onClick(View v) {
		if (v.getId() == _addContactButton.getId()) {
			Intent intent = new Intent(Intent.ACTION_PICK,
					ContactsContract.CommonDataKinds.Phone.CONTENT_URI);
			startActivityForResult(intent, 101);
		} else if (v.getId() == _commitName.getId()) {
			new CommitName().execute(_groupName.getText().toString());
		}
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.Activity#onCreateContextMenu(android.view.ContextMenu,
	 * android.view.View, android.view.ContextMenu.ContextMenuInfo)
	 */
	@Override
	public void onCreateContextMenu(ContextMenu menu, View v,
			ContextMenuInfo menuInfo) {
		// This fires on press and hold from the list activity.
		// This will not fire unless you register a view for context menu (see
		// our onCreate implementation).
		super.onCreateContextMenu(menu, v, menuInfo);
		final MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.delete_context_menu, menu);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.Activity#onContextItemSelected(android.view.MenuItem)
	 */
	@Override
	public boolean onContextItemSelected(MenuItem item) {
		// Grab the info about what item prompted the context menu
		AdapterView.AdapterContextMenuInfo info;
		try {
			info = (AdapterView.AdapterContextMenuInfo) item.getMenuInfo();
		} catch (ClassCastException e) {
			return false;
		}
		// grab the contact associated with the long press
		GroupContact tmp = (GroupContact) getListAdapter().getItem(
				info.position);

		// remove the row from the DB
		new RemoveContact().execute(tmp);

		return true;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.Activity#onCreateOptionsMenu(android.view.Menu)
	 */
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// This displays our options menu (menu from the bottom)
		// when the user presses their built in menu button.
		final MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.add_contact_to_group, menu);
		return true;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.Activity#onOptionsItemSelected(android.view.MenuItem)
	 */
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// This intent is polling our contacts for all contacts with phone
		// numbers
		// Most of this functionality is for free, but we need to start the
		// intent for a result and make sure to
		// listen for the results in "onActivityResult" implemented below.
		if (item.getItemId() == R.id.add_contact_to_group) {
			Intent intent = new Intent(Intent.ACTION_PICK,
					ContactsContract.CommonDataKinds.Phone.CONTENT_URI);
			startActivityForResult(intent, ADD_CONTACT);

		}
		return true;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.Activity#onActivityResult(int, int,
	 * android.content.Intent)
	 */
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		super.onActivityResult(requestCode, resultCode, data);
		if (requestCode == ADD_CONTACT && resultCode == RESULT_OK) {
			// Grab the raw data from our intent result
			Uri contact = data.getData();

			// We need to use a cursor to query the data. It's quicker to ask
			// the cursor for specific data at the
			// time of the query but this works fine too.
			Cursor c = getContentResolver().query(contact, null, null, null,
					null);
			if (c.moveToFirst()) {
				// We'll grab the column number from the Constant in the
				// ContactContract class then grab the value.

				// our Cursor columns include everything in
				// ContactsContract.Data...
				int myColumn = c
						.getColumnIndex(ContactsContract.Data.DISPLAY_NAME);
				String displayName = c.getString(myColumn);
				myColumn = c.getColumnIndex(ContactsContract.Data.CONTACT_ID);
				int id = c.getInt(myColumn);

				// and everything in ContactsContract.CommonDataKinds.Phone
				myColumn = c
						.getColumnIndex(ContactsContract.CommonDataKinds.Phone.NUMBER);
				String phoneNumber = c.getString(myColumn);

				// Create a newContact without the unique _id
				GroupContact newContact = new GroupContact(_currentGroup,
						displayName, phoneNumber, id);

				// update the group in the background thread.
				new AddContactToGroup().execute(newContact);

			}

			// always close the cursor when you're finished.
			c.close();
		}
	}

	class RemoveContact extends AsyncTask<GroupContact, Void, Void> {

		@Override
		protected Void doInBackground(GroupContact... params) {
			GMDBController dbController = GMDBController
					.openWithContext(EditGroupActivity.this);
			try {
				dbController.removeContactFromGroup(_currentGroup, params[0]);
			} catch (Exception e) {
				e.printStackTrace();
			} finally {
				dbController.close();
			}

			return null;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see android.os.AsyncTask#onPostExecute(java.lang.Object)
		 */
		@Override
		protected void onPostExecute(Void result) {
			super.onPostExecute(result);
			// refill the listview
			new GetContactsForGroup().execute();
		}

	}

	class CreateNewGroup extends AsyncTask<Void, Void, Void> {

		@Override
		protected Void doInBackground(Void... params) {
			GMDBController dbController = GMDBController
					.openWithContext(EditGroupActivity.this);
			try {
				_currentGroup = (int) dbController.createContactGroup();
			} catch (Exception e) {
				e.printStackTrace();
			} finally {
				dbController.close();
			}

			return null;
		}

	}

	/*
	 * Change the name of the group in a background thread.
	 * 
	 * See the DB controller for details
	 */
	class CommitName extends AsyncTask<String, Void, Void> {

		@Override
		protected Void doInBackground(String... params) {
			GMDBController dbController = GMDBController
					.openWithContext(EditGroupActivity.this);
			try {
				dbController.updateGroupName(_currentGroup, params[0]);
			} catch (Exception e) {
				e.printStackTrace();
			} finally {
				dbController.close();
			}

			return null;
		}

		/* (non-Javadoc)
		 * @see android.os.AsyncTask#onPostExecute(java.lang.Object)
		 */
		@Override
		protected void onPostExecute(Void result)
			{
			super.onPostExecute(result);
			Toast.makeText(EditGroupActivity.this, "Group name changed", Toast.LENGTH_SHORT).show();
			}
		
		

	}

	/*
	 * Once we have the contact object, add it to the database in a background
	 * thread.
	 */
	class AddContactToGroup extends AsyncTask<GroupContact, Void, Void> {

		@Override
		protected Void doInBackground(GroupContact... params) {
			GMDBController dbController = GMDBController
					.openWithContext(EditGroupActivity.this);
			try {
				dbController.addContactToGroup(_currentGroup, params[0]);
			} catch (Exception e) {
				e.printStackTrace();
			} finally {
				dbController.close();
			}

			return null;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see android.os.AsyncTask#onPostExecute(java.lang.Object)
		 */
		@Override
		protected void onPostExecute(Void result) {
			super.onPostExecute(result);
			// refill the list afterwards.
			new GetContactsForGroup().execute();
		}

	}

	/**
	 * An asyncTask to query the database for our list of groups.
	 * 
	 * When finished with the query it will populate the list via a new Adapter
	 * 
	 */
	class GetContactsForGroup
			extends
				AsyncTask<Void, Void, ArrayList<GroupContact>> {

		@Override
		protected ArrayList<GroupContact> doInBackground(Void... params) {
			ArrayList<GroupContact> myContacts = null;

			GMDBController dbController = GMDBController
					.openWithContext(EditGroupActivity.this);
			try {
				myContacts = dbController.getGroupContacts(_currentGroup);
			} catch (Exception e) {
				e.printStackTrace();
			} finally {
				dbController.close();
			}

			return myContacts;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see android.os.AsyncTask#onPostExecute(java.lang.Object)
		 */
		@Override
		protected void onPostExecute(ArrayList<GroupContact> result) {
			// Array adapter is used in conjunction with the MessageGroup (and
			// it's toString method)

			setListAdapter(new ArrayAdapter<GroupContact>(
					EditGroupActivity.this,
					android.R.layout.simple_list_item_1, result));
		}

	}

}
