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
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.ContextMenu;
import android.view.ContextMenu.ContextMenuInfo;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;

import com.microsoft.android.example.groupmessage.data.GMDBController;
import com.microsoft.android.example.groupmessage.data.MessageGroup;

public class GroupsActivity extends ListActivity implements OnClickListener {

	TextView _noResults;

	/** Called when the activity is first created. */
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.main);

		_noResults = (TextView) findViewById(R.id.empty_list_add_group);
		_noResults.setOnClickListener(this);

		registerForContextMenu(getListView());
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.ListActivity#onListItemClick(android.widget.ListView,
	 * android.view.View, int, long)
	 */
	@Override
	protected void onListItemClick(ListView l, View v, int position, long id) {
		super.onListItemClick(l, v, position, id);

		MessageGroup tmp = (MessageGroup) l.getItemAtPosition(position);

		Intent sendMessage = new Intent(this, SendMessageActivity.class);
		sendMessage.putExtra("GROUP_ID", tmp.getId());
		startActivity(sendMessage);

	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.Activity#onCreateOptionsMenu(android.view.Menu)
	 * 
	 * Inflate the menu (from our own menu xml file)
	 * 
	 * This even fires when the user presses the menu button on their phone
	 */
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		final MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.add_group, menu);
		return true;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.Activity#onOptionsItemSelected(android.view.MenuItem)
	 */
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// Opening a new activity without passing a Group Id will prompt the
		// edit Activity to create a new one.
		Intent editGroup = new Intent(this, EditGroupActivity.class);
		startActivity(editGroup);
		return true;
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
		super.onCreateContextMenu(menu, v, menuInfo);
		final MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.group_context_menu, menu);
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
		MessageGroup tmp = (MessageGroup) getListAdapter().getItem(
				info.position);

		// Get the context menu item details (edit or delete)
		if (item.getItemId() == R.id.remove_group) {
			new RemoveGroup().execute(tmp);
		} else {
			// Pass in the group Id and the group name.

			// The group name we only care about for display reasons. The ID
			// drives our lists.
			Intent editGroup = new Intent(this, EditGroupActivity.class);
			editGroup.putExtra("GROUP_ID", tmp.getId());
			editGroup.putExtra("GROUP_NAME", tmp.getName());
			startActivity(editGroup);
		}
		return true;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.Activity#onResume()
	 */
	@Override
	protected void onResume() {
		super.onResume();
		new GetGroupsAsync().execute();
	}

	// The only onclick we listen for is the empty list button.
	public void onClick(View v) {
		Intent editGroup = new Intent(this, EditGroupActivity.class);
		startActivity(editGroup);
	}

	/**
	 * An asyncTask to query the database for our list of groups.
	 * 
	 * When finished with the query it will populate the list via a new Adapter
	 * 
	 */
	class GetGroupsAsync extends AsyncTask<Void, Void, ArrayList<MessageGroup>> {

		@Override
		protected ArrayList<MessageGroup> doInBackground(Void... params) {
			ArrayList<MessageGroup> messageGroups = null;

			GMDBController dbController = GMDBController
					.openWithContext(GroupsActivity.this);
			try {
				messageGroups = dbController.getMessageGroups();
			} catch (Exception e) {
				e.printStackTrace();
			} finally {
				dbController.close();
			}

			return messageGroups;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see android.os.AsyncTask#onPostExecute(java.lang.Object)
		 */
		@Override
		protected void onPostExecute(ArrayList<MessageGroup> result) {
			// Array adapter is used in conjunction with the MessageGroup (and
			// it's toString method)
			setListAdapter(new ArrayAdapter<MessageGroup>(GroupsActivity.this,
					android.R.layout.simple_list_item_1, result));
		}

	}

	class RemoveGroup extends AsyncTask<MessageGroup, Void, Void> {

		@Override
		protected Void doInBackground(MessageGroup... params) {
			GMDBController dbController = GMDBController
					.openWithContext(GroupsActivity.this);
			try {
				dbController.removeMessageGroup(params[0].getId());
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
			new GetGroupsAsync().execute();
		}

	}

}