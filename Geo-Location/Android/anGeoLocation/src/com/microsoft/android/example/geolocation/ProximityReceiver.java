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

package com.microsoft.android.example.geolocation;

import android.app.AlertDialog;
import android.app.AlertDialog.Builder;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.location.LocationManager;

/**
 * Simple handler for when user is sufficiently close to the selected location.
 */
public class ProximityReceiver extends BroadcastReceiver {
	@Override
	public void onReceive(Context context, Intent intent) {
		// ensure the intent is sent with they are entering the location (and
		// not exiting)
		if (intent.getBooleanExtra(LocationManager.KEY_PROXIMITY_ENTERING,
				false)) {
			// use builder to create alert dialog.
			Builder b = new AlertDialog.Builder(context);
			b.setTitle("You Have Arrived");
			b.setMessage("Your current location is on your pin.");
			b.setCancelable(true);
			b.setPositiveButton("OK", new DialogInterface.OnClickListener() {
				public void onClick(DialogInterface dialog, int which) {
					// clicking OK just closes modal dialog.
					dialog.cancel();
				}
			});

			// create and show dialog
			AlertDialog ad = b.create();
			ad.show();
		}
	}
}
