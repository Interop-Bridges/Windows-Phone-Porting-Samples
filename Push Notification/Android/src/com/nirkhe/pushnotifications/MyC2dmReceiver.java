package com.nirkhe.pushnotifications;
import java.util.List;

import android.app.AlertDialog;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences.Editor;
import android.os.Bundle;
import android.util.Log;
import android.widget.CheckBox;
import android.widget.Toast;

public class MyC2dmReceiver extends BroadcastReceiver {
	private static String KEY = "c2dmPref";
	private static String REGISTRATION_KEY = "registrationKey";

	public static final String C2DMREGFILTER = "com.nirkhe.intent.regfilter";
    public static final String DEVICEIDSTR = "deviceid";
	public static final String C2DMMESGFILTER = "com.nirkhe.intent.mesgfilter";
    public static final String MESSAGESTR = "message";
    public static final String COUNTSTR = "count";
    public static final String SOUNDSTR = "sound";
    public static final String TYPESTR = "type";

    private Context context;
	@Override
	public void onReceive(Context context, Intent intent) {
	    this.context = context;
		if (intent.getAction().equals("com.google.android.c2dm.intent.REGISTRATION")) {
	        handleRegistration(context, intent);
	    } else if (intent.getAction().equals("com.google.android.c2dm.intent.RECEIVE")) {
	        handleMessage(context, intent);
	    }
	 }
	private void handleRegistration(Context context, Intent intent) {
	    String registration = intent.getStringExtra("registration_id");
	    if (intent.getStringExtra("error") != null) {
	        // Registration failed, should try again later.
		    Log.d("c2dm", "registration failed");
		    String error = intent.getStringExtra("error");
		    if(error == "SERVICE_NOT_AVAILABLE"){
		    	Log.d("c2dm", "SERVICE_NOT_AVAILABLE");
		    }else if(error == "ACCOUNT_MISSING"){
		    	Log.d("c2dm", "ACCOUNT_MISSING");
		    }else if(error == "AUTHENTICATION_FAILED"){
		    	Log.d("c2dm", "AUTHENTICATION_FAILED");
		    }else if(error == "TOO_MANY_REGISTRATIONS"){
		    	Log.d("c2dm", "TOO_MANY_REGISTRATIONS");
		    }else if(error == "INVALID_SENDER"){
		    	Log.d("c2dm", "INVALID_SENDER");
		    }else if(error == "PHONE_REGISTRATION_ERROR"){
		    	Log.d("c2dm", "PHONE_REGISTRATION_ERROR");
		    }
	    } else if (intent.getStringExtra("unregistered") != null) {
	        // unregistration done, new messages from the authorized sender will be rejected
	    	Log.d("c2dm", "unregistered");

	    } else if (registration != null) {
	    	Log.d("c2dm", registration);
	    	Editor editor =
                context.getSharedPreferences(KEY, Context.MODE_PRIVATE).edit();
            editor.putString(REGISTRATION_KEY, registration);
    		editor.commit();
    		
    		Intent c2dmReceivedIntent = new Intent();
    		c2dmReceivedIntent.setAction(C2DMREGFILTER);
    		c2dmReceivedIntent.putExtra(DEVICEIDSTR, registration);
    		context.sendBroadcast(c2dmReceivedIntent);

	       // Send the registration ID to the 3rd party site that is sending the messages.
	       // This should be done in a separate thread.
	       // When done, remember that all registration is done.
	    }
	}
	
	private void showAlert(Context context, String message)
	{
        AlertDialog.Builder alertbox = new AlertDialog.Builder(context);
        final Context tcontext = context; 
        // set the message to display
        alertbox.setMessage("This is the alertbox!");

        // set a positive/yes button and create a listener
        alertbox.setPositiveButton("Yes", new DialogInterface.OnClickListener() {

            // do something when the button is clicked
            public void onClick(DialogInterface arg0, int arg1) {
                Toast.makeText(tcontext, "'Yes' button clicked", Toast.LENGTH_SHORT).show();
            }
        });

        // set a negative/no button and create a listener
        alertbox.setNegativeButton("No", new DialogInterface.OnClickListener() {

            // do something when the button is clicked
            public void onClick(DialogInterface arg0, int arg1) {
                Toast.makeText(tcontext, "'No' button clicked", Toast.LENGTH_SHORT).show();
            }
        });

        // display box
        alertbox.show();
	}
	
	
	private void handleMessage(Context context, Intent intent)
	{
		int i=0;
		Bundle extras = intent.getExtras();
	    if (extras != null) {
	        String msg = extras.get(MESSAGESTR) + "";
	        String count = extras.get(COUNTSTR) + "";
	        String sound = extras.get(SOUNDSTR) + "";
	        String typeStr = extras.get(TYPESTR) + "";
	        Log.d("Message", msg);
	        // Now do something smart based on the information

			Intent c2dmReceivedIntent = new Intent();
			c2dmReceivedIntent.setAction(C2DMMESGFILTER);
			c2dmReceivedIntent.putExtra(MESSAGESTR, msg);
			c2dmReceivedIntent.putExtra(COUNTSTR, count);
			c2dmReceivedIntent.putExtra(SOUNDSTR, sound);
			c2dmReceivedIntent.putExtra(TYPESTR, typeStr);
			context.sendBroadcast(c2dmReceivedIntent);
	        //Toast.makeText(context, msg, Toast.LENGTH_LONG).show();
			//this.showAlert(context, msg);
	        

	    }
	}
	
}
