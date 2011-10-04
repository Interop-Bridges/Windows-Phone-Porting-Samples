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

import android.app.Activity;
import android.app.AlertDialog;
import android.app.AlertDialog.Builder;
import android.content.DialogInterface;
import android.content.res.Resources;
import android.os.AsyncTask;
import android.os.Bundle;
import android.telephony.SmsManager;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;

import com.microsoft.android.example.groupmessage.data.GMDBController;
import com.microsoft.android.example.groupmessage.data.GroupContact;

public class SendMessageActivity extends Activity implements OnClickListener {

	private int _currentGroup;

	private Button _sendMessage;
	private ArrayList<GroupContact> _contactsForGroup;
	private EditText _messageBody;
	/*
	 * (non-Javadoc)
	 * 
	 * @see android.app.Activity#onCreate(android.os.Bundle)
	 */
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.send_message);

		Bundle extras = getIntent().getExtras();
		_currentGroup = extras.getInt("GROUP_ID", -1);

		// setup the button but disable it until we are ready to go.
		_sendMessage = (Button) findViewById(R.id.executeSend);
		_sendMessage.setOnClickListener(this);
		_sendMessage.setEnabled(false);

		_messageBody = (EditText) findViewById(R.id.messageBodyField);
		new GetContactsForGroup().execute();

	}

	public void onClick(View v) {
		if (v.getId() == _sendMessage.getId()) {

			// Look through our contacts and send them the SMS. This requires
			// the SEND_SMS privilege found in the manifest.
			for (GroupContact contact : _contactsForGroup) {
				SmsManager.getDefault().sendTextMessage(contact.getPhone(),
						null, _messageBody.getText().toString(), null, null);
			}
		}
		finish();
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
					.openWithContext(SendMessageActivity.this);
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

			// If the list of contacts is empty, we just return and
			// display a message.
			if (result == null || result.size() < 1) {
				Resources r = getResources();
				Builder b = new AlertDialog.Builder(SendMessageActivity.this);

				b.setTitle(r.getString(R.string.no_contacts_in_group));
				b.setMessage(r.getString(R.string.add_contacts_first));
				b.setCancelable(false);
				b.setPositiveButton(r.getString(R.string.ok),
						new DialogInterface.OnClickListener() {
							public void onClick(DialogInterface dialog,
									int which) {
								dialog.cancel();
								finish();
								return;
							}
						});
				AlertDialog ad = b.create();
				ad.show();
			} else {
				// Enable the send message button and setup the contact list.
				_sendMessage.setEnabled(true);
				_contactsForGroup = result;
			}
		}

	}

}
