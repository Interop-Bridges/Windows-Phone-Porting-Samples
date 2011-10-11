package com.nirkhe.pushnotifications;

import java.io.InputStream;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;

import android.app.Activity;

import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.sax.Element;
import android.util.Log;

import javax.xml.parsers.*;
import org.w3c.dom.Document; 
import org.w3c.dom.Node;
import org.w3c.dom.NodeList; 
import org.xml.sax.InputSource; 

public class SubscriptionsParser 	 {
    /** Called when the activity is first created. */
	public List<Subscription> parse(String xmlstr) {
        DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
        Document xmlDocument = null;
        List<Subscription> ls = new ArrayList<Subscription>();
        factory.setNamespaceAware(true); 
            try {
            	StringReader reader = new StringReader(xmlstr );
            	InputSource inputSource = new InputSource( reader );
            	DocumentBuilder parser = factory.newDocumentBuilder(); 
                xmlDocument = parser.parse(inputSource); 
            	reader.close();                
            } catch (Exception e) { 
            	Log.d("not an XML string ", xmlstr);
            	return null;
            } 
            org.w3c.dom.Element root = null;
            if (xmlDocument != null)
            	 root = xmlDocument.getDocumentElement(); 
            if (root == null) { 
            	Log.d("error parsing XML string ", xmlstr);
            	return null;
            }
            NodeList subscriptions = root.getElementsByTagName("DeviceSubscriptionInfo");
            for (int i=0; i<subscriptions.getLength(); i++){
            	org.w3c.dom.Element ele = (org.w3c.dom.Element)  subscriptions.item(i);
                NodeList NameNodes = ele.getElementsByTagName("Name");
                org.w3c.dom.Element nameNode = (org.w3c.dom.Element)  NameNodes.item(0);
                Node tNode = nameNode.getFirstChild();
                String name = tNode.getNodeValue();
            	
                NodeList DescriptionNodes = ele.getElementsByTagName("Description");
                org.w3c.dom.Element DescriptionNode = (org.w3c.dom.Element)  DescriptionNodes.item(0);
                tNode = DescriptionNode.getFirstChild();
                String description = tNode.getNodeValue();

                NodeList IsSubscribedNodes = ele.getElementsByTagName("IsSubscribed");
                org.w3c.dom.Element IsSubscribedNode = (org.w3c.dom.Element)  IsSubscribedNodes.item(0);
                tNode = IsSubscribedNode.getFirstChild();
                String isSubscribedStr = tNode.getNodeValue();
                Boolean isSubscribed = false;
                if (isSubscribedStr.equals("true"))
                	isSubscribed = true;

                Subscription sub = new Subscription();
                sub.Name = name;
                sub.Description = description;
                sub.IsSubscribed = isSubscribed;
                ls.add(sub);
            	Log.d("Sub info: ", "name:"+ name + " description:"+ description + " IsSubscribed:" +isSubscribedStr );


            }
            return ls;
	}

}