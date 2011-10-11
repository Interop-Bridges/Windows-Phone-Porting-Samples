package com.nirkhe.pushnotifications;

import java.util.List;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Toast;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.LinearLayout;


public class PushNotifications extends Activity  implements CheckBox.OnCheckedChangeListener{
    //private static String SERVICE_URL="http://10.0.2.2:84/AndroidDevice.svc";
    //private static String PUSHSERVICE_URL="http://10.0.2.2:84/push.svc";
    private static String SERVICE_URL="http://wp7azuretest1.cloudapp.net/AndroidDevice.svc";
    private static String PUSHSERVICE_URL="http://wp7azuretest1.cloudapp.net/push.svc";

    public static final String PUSHPREFS_FILE = "PushReceiverPrefsFile";
    public static final String DEVICEID_PREF = "DeviceID";
    
    private String deviceID;

    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        
        // Restore preferences
        SharedPreferences settings = getSharedPreferences(PUSHPREFS_FILE, 0);
        String deviceID_pref = settings.getString(DEVICEID_PREF, null);
        
        if (deviceID_pref == null) {
        	Intent registrationIntent = new Intent("com.google.android.c2dm.intent.REGISTER");
        	registrationIntent.putExtra("app", PendingIntent.getBroadcast(this, 0, new Intent(), 0));
        	registrationIntent.putExtra("sender", "vivek_nirkhe@hotmail.com");
        	this.startService(registrationIntent);
        } else {
        	deviceID = deviceID_pref;
    		registerDevice(deviceID);
            showSubscriptions(deviceID);
            Log.d("deviceID", deviceID);
        }
        	
		//Register Broadcast Receiver
		IntentFilter filter = new IntentFilter(MyC2dmReceiver.C2DMREGFILTER);
		registerReceiver(myReceiver, filter);

		IntentFilter msgfilter = new IntentFilter(MyC2dmReceiver.C2DMMESGFILTER);
		registerReceiver(messageReceiver, msgfilter);

		
/*		LinearLayout.LayoutParams p = new LinearLayout.LayoutParams(
                LinearLayout.LayoutParams.MATCH_PARENT,
                LinearLayout.LayoutParams.WRAP_CONTENT
        );		
        LinearLayout layout = (LinearLayout) findViewById(R.id.linearlayout);
        Button buttonView = new Button(this);
        buttonView.setText("Button ");
        layout.addView(buttonView, p);
*/
    }
    
	private BroadcastReceiver messageReceiver = new BroadcastReceiver() {
		@Override
		public void onReceive(Context context, Intent intent) {
			Bundle extras = intent.getExtras();
			// Unwrap bundle object
			String message = extras.getString(MyC2dmReceiver.MESSAGESTR);// get the counter value 
			String sound = extras.getString(MyC2dmReceiver.SOUNDSTR);// get the counter value 
			String countStr = extras.getString(MyC2dmReceiver.COUNTSTR);// get the counter value
			String typeStr = extras.getString(MyC2dmReceiver.TYPESTR);// get the counter value
			int count = 0;
			if (countStr.compareTo("") != 0) {
				try {
					count = Integer.parseInt(countStr);
				} catch (NumberFormatException e)
				{
					count = 0;
				}
			}
			//running inside Service	
			Log.d("message", message);

			if (typeStr.compareTo("raw") == 0) {
				AlertDialog alertDialog = new AlertDialog.Builder(context).create();
				alertDialog.setTitle("Azure Notification");
				alertDialog.setMessage(message);
				alertDialog.setButton("OK", new DialogInterface.OnClickListener() {
					public void onClick(DialogInterface dialog, int which) {
						return;
					} });
				alertDialog.show();			
			} else {
				String ns = Context.NOTIFICATION_SERVICE;
				NotificationManager mNotificationManager = (NotificationManager) getSystemService(ns);
				
				int icon = R.drawable.icon;
				CharSequence tickerText = message;
				long when = System.currentTimeMillis();

				Notification notification = new Notification(icon, tickerText, when);
				notification.flags = Notification.FLAG_AUTO_CANCEL;
				if (sound.compareTo("") != 0)
				{
					notification.defaults |= Notification.DEFAULT_SOUND; 
				}
				if (count != 0)
				{
					notification.number = count;			
				}
				
				Context context1 = getApplicationContext();
				CharSequence contentTitle = "Azure Notification";
				CharSequence contentText = message;
				Intent notificationIntent = new Intent();
				PendingIntent contentIntent = PendingIntent.getActivity(context1, 0, notificationIntent, 0);

				notification.setLatestEventInfo(context, contentTitle, contentText, contentIntent);
				
				final int HELLO_ID = 1;

				mNotificationManager.notify(HELLO_ID, notification);				
			}
		}	
	};
		
	private BroadcastReceiver myReceiver = new BroadcastReceiver() {
		
		@Override
		public void onReceive(Context context, Intent intent) {
			// TODO Auto-generated method stub
			//Get Bundles
			Bundle extras = intent.getExtras();
			String registrationId;
			// Unwrap bundle object
			registrationId = extras.getString(MyC2dmReceiver.DEVICEIDSTR);// get the counter value 
			deviceID = registrationId;
			//running inside Service	
			Log.d("deviceID", registrationId);
			
		      SharedPreferences settings = getSharedPreferences(PUSHPREFS_FILE, 0);
		      SharedPreferences.Editor editor = settings.edit();
		      editor.putString(DEVICEID_PREF, registrationId);
		      // Commit the edits!
		      editor.commit();
			
    		registerDevice(registrationId);
            showSubscriptions(registrationId);

		}
	}; 
	
	private void showSubscriptions(String registration)
	{
        String deviceSubsUrl = PUSHSERVICE_URL + "/subs/" + registration;
		//String deviceAddUrl = "http://10.0.2.2:82/push.svc/subs";
    	Log.d("server - device Subs URL", deviceSubsUrl);

		RestClient client = new RestClient(deviceSubsUrl);
		 
		try {
		    client.Execute(RequestMethod.GET);
		} catch (Exception e) {
		    e.printStackTrace();
		}		 
		String response = client.getResponse();
    	Log.d("server - response", response);
    	SubscriptionsParser sp = new SubscriptionsParser();
    	List<Subscription> ls = sp.parse(response);
        LinearLayout layout = (LinearLayout) findViewById(R.id.linearlayout);
        if (ls != null) {
	    	for (int i=0; i<ls.size(); i++) {
	    		Subscription s = ls.get(i);
	    		
	    		CheckBox chk = new CheckBox(this);
	    		chk.setChecked(s.IsSubscribed);
	    		chk.setText(s.Name);
	    		chk.setOnCheckedChangeListener((OnCheckedChangeListener) this);    		
	    		layout.addView(chk);
	    		
	//            LinearLayout.LayoutParams p = new LinearLayout.LayoutParams(
	//                    LinearLayout.LayoutParams.MATCH_PARENT,
	//                    LinearLayout.LayoutParams.WRAP_CONTENT
	//            );
	//            Button buttonView = new Button(this);
	//            buttonView.setText("Button " + i);
	            //layout.addView(buttonView, p);
	    	}
        }
	}

	private void subscribe(String subscription)
	{
        String subscriptionAddUrl = PUSHSERVICE_URL + "/sub/add/" + subscription+"/"+deviceID;
		//String deviceAddUrl = "http://10.0.2.2:82/push.svc/subs";
    	Log.d("server - device URL", subscriptionAddUrl);

		RestClient client = new RestClient(subscriptionAddUrl);
		 
		try {
		    client.Execute(RequestMethod.POST);
		    //client.Execute(RequestMethod.GET);
		} catch (Exception e) {
		    e.printStackTrace();
		}		 
		String response = client.getResponse();
    	Log.d("server - response", response);		
	}
	private void unsubscribe(String subscription)
	{
        String subscriptionAddUrl = PUSHSERVICE_URL + "/sub/delete/" + subscription+"/"+deviceID;
		//String deviceAddUrl = "http://10.0.2.2:82/push.svc/subs";
    	Log.d("server - device URL", subscriptionAddUrl);

		RestClient client = new RestClient(subscriptionAddUrl);
		 
		try {
		    client.Execute(RequestMethod.POST);
		    //client.Execute(RequestMethod.GET);
		} catch (Exception e) {
		    e.printStackTrace();
		}		 
		String response = client.getResponse();
    	Log.d("server - response", response);		
	}
	
	
	@Override
	public void onCheckedChanged(CompoundButton cb, boolean isChecked )
	{
		int i=0;
		String sub = (String) cb.getText();
		if (cb.isChecked())
		{
			this.subscribe(sub);
		}
		else {
			this.unsubscribe(sub);
		}
	}

	private void registerDevice(String registration)
	{
        String deviceAddUrl = SERVICE_URL + "/register/" + registration;
		//String deviceAddUrl = "http://10.0.2.2:82/push.svc/subs";
    	Log.d("server - device URL", deviceAddUrl);

		RestClient client = new RestClient(deviceAddUrl);
		 
		try {
		    client.Execute(RequestMethod.POST);
		    //client.Execute(RequestMethod.GET);
		} catch (Exception e) {
		    e.printStackTrace();
		}		 
		String response = client.getResponse();
    	Log.d("server - response", response);
	}

}