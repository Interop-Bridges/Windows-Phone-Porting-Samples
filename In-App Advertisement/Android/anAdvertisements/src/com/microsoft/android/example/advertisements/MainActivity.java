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

package com.microsoft.android.example.advertisements;

import android.app.Activity;
import android.os.Bundle;

import com.admob.android.ads.AdView;

public class MainActivity extends Activity {
    
    // 30 second ad interval
    private static final int AD_INTERVAL = 30; 
    private AdView mAdView;

    /**
     * TODO : NOTE : Your AdMob API key must be entered into the manifest file where indicated.
     * 
     * Go to your manifest file and replace "yourApiKeyHere" with your specific AdMob API key.
     * To create one you must go to http://www.admob.com/ and setup an account which will require
     * your bank information as well as tax information.
     */
    
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        
        mAdView = (AdView)findViewById(R.id.main_ad);
        
        // can set default keywords for advertisement targeting...
        mAdView.setKeywords("a list of keywords");
        
        // update interval for ads
        mAdView.setRequestInterval(AD_INTERVAL);
        
        // request your ad, will not show add without API key
        mAdView.requestFreshAd();
    }
}