// --------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft Corp">
// Copyright 2010 Microsoft Corp
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//  http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// ---------------------------------------------------------------------
namespace PushMessageDataProvider
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    /// <summary>
    /// Used parse data returned by the service. 
    /// </summary>
    public struct SubscriptionInfo
    {
        /// <summary>
        /// Gets or sets the Subscription Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the subscription description.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Utils class includes bunch of utility routines
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Sets the cert policy.
        /// </summary>
        public static void SetCertificatePolicy()
        {
            ServicePointManager.ServerCertificateValidationCallback
                       += RemoteCertificateValidate;
        }

        /// <summary>
        /// Remotes the certificate validate.
        /// </summary>
        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // trust any certificate!!!
            System.Console.WriteLine("Warning, trust any certificate");
            return true;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
         //After deploying to address, change the BaseAddress to the address of the deployed AzurePushMessage service
         //private const string BaseAddress = "http://azurepushtest1.cloudapp.net/push.svc";
         //private const string BaseAddressWP7Service = "http://azurepushtest1.cloudapp.net/WP7Device.svc";

         private const string BaseAddress = "http://127.0.0.1/push.svc";
         private const string BaseAddressWP7Service = "http://127.0.0.1/WP7Device.svc";


        private const string Username = "tony";
        private const string Password = "clifton";

        private List<string> subscriptionsList;

        /// <summary>
        /// Initializes a new instance of the MainWindow class. 
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.sendBtn.IsEnabled = false;
            Utils.SetCertificatePolicy();
        }

        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes
                = System.Text.UTF8Encoding.UTF8.GetBytes(toEncode);
            string returnValue
                  = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        private static string SendPostRequest(string url, string postData)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(postData);

            // Prepare web request...
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Credentials = new NetworkCredential(Username, Password);
            request.PreAuthenticate = true;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            Stream newStream = request.GetRequestStream();

            // Send the data.
            newStream.Write(data, 0, data.Length);
            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                return "Exception : " + e.Message;
            }
        }

        private static string SendGetRequest(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Credentials = new NetworkCredential(Username, Password);
            request.PreAuthenticate = true;

            // Get response  
            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    // Get the response stream  
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                return "Exception : " + e.Message;
            }
        }

        private void SubCreateBtnClick(object sender, RoutedEventArgs e)
        {
            statusTextBlock.Text = string.Empty;
            string subNameStr = subNameTextBox.Text;
            string descStr = descTextBox.Text;
            string status = SendPostRequest(BaseAddress + "/sub/create/" + subNameStr + "?desc=" + descStr, string.Empty);
            statusTextBlock.Text = status;
        }

        private void TabItem2MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string subStr = SendGetRequest(BaseAddress + "/subs");
                if (!string.IsNullOrEmpty(subStr))
                {
                    XDocument xdoc = XDocument.Parse(subStr);
                    IEnumerable<XElement> xelements = xdoc.Descendants();

                    var subscriptions = from element in xdoc.Descendants("{http://schemas.datacontract.org/2004/07/MsgHelperLib.Common}SubscriptionInfo")
                                        select new SubscriptionInfo
                                        {
                                            Name = element.Element("{http://schemas.datacontract.org/2004/07/MsgHelperLib.Common}Name").Value,
                                            Description = element.Element("{http://schemas.datacontract.org/2004/07/MsgHelperLib.Common}Description").Value,
                                        };
                    subscriptionsComboBox.ItemsSource = subscriptions;
                    subscriptionsComboBox.SelectedIndex = 0;
                    this.subscriptionsList = new List<string>();
                    foreach (SubscriptionInfo si in subscriptions)
                    {
                        this.subscriptionsList.Add(si.Name);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void ToastBtn_Checked(object sender, RoutedEventArgs e)
        {
            countTxtBlk.IsEnabled = false;
            countTxtBox.IsEnabled = false;
            soundTxtBlk.IsEnabled = false;
            soundTxtBox.IsEnabled = false;
            imgTxtBlk.IsEnabled = false;
            imgTxtBox.IsEnabled = false;
            sendBtn.IsEnabled = true;
            msgStatusLbl.Content = string.Empty;
        }

        private void TileBtn_Checked(object sender, RoutedEventArgs e)
        {
            countTxtBlk.IsEnabled = true;
            countTxtBox.IsEnabled = true;
            soundTxtBlk.IsEnabled = false;
            soundTxtBox.IsEnabled = false;
            imgTxtBlk.IsEnabled = true;
            imgTxtBox.IsEnabled = true;
            sendBtn.IsEnabled = true;
            msgStatusLbl.Content = string.Empty;
        }

        private void RawBtn_Checked(object sender, RoutedEventArgs e)
        {
            countTxtBlk.IsEnabled = false;
            countTxtBox.IsEnabled = false;
            soundTxtBlk.IsEnabled = false;
            soundTxtBox.IsEnabled = false;
            imgTxtBlk.IsEnabled = false;
            imgTxtBox.IsEnabled = false;
            sendBtn.IsEnabled = true;
            msgStatusLbl.Content = string.Empty;
        }

        private void iOSBtn_Checked(object sender, RoutedEventArgs e)
        {
            countTxtBlk.IsEnabled = true;
            countTxtBox.IsEnabled = true;
            soundTxtBlk.IsEnabled = true;
            soundTxtBox.IsEnabled = true;
            imgTxtBlk.IsEnabled = false;
            imgTxtBox.IsEnabled = false;
            sendBtn.IsEnabled = true;
            msgStatusLbl.Content = string.Empty;
        }

        private void commonBtn_Checked(object sender, RoutedEventArgs e)
        {
            countTxtBlk.IsEnabled = true;
            countTxtBox.IsEnabled = true;
            soundTxtBlk.IsEnabled = true;
            soundTxtBox.IsEnabled = true;
            imgTxtBlk.IsEnabled = true;
            imgTxtBox.IsEnabled = true;
            sendBtn.IsEnabled = true;
            msgStatusLbl.Content = string.Empty;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)toastBtn.IsChecked)
            {
                this.SendToastMessage();
            }
            else if ((bool)tileBtn.IsChecked)
            {
                this.SendTileMessage();
            }
            else if ((bool)rawBtn.IsChecked)
            {
                this.SendRawMessage();
            }
            else if ((bool)iOSBtn.IsChecked)
            {
                this.SendiOSMessage();
            }
            else if ((bool)commonBtn.IsChecked)
            {
                this.SendCommonMessage();
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            string sub = this.GetSelectedSubscription();
            string returnStr = SendPostRequest(BaseAddress + "/sub/delete/" + sub, string.Empty);
            msgStatusLbl.Content = returnStr;
        }

        private void SendCommonMessage()
        {
            msgStatusLbl.Content = string.Empty;
            try
            {
                string msg = msgTxtBox.Text;
                string img = imgTxtBox.Text;
                string sound = soundTxtBox.Text;
                int count;
                if (int.TryParse(countTxtBox.Text, out count) || string.IsNullOrEmpty(countTxtBox.Text))
                {
                    string sub = this.GetSelectedSubscription();
                    SendPostRequest(BaseAddress + "/message/common/" + sub + "?mesg=" + msg + "&count=" + count.ToString(CultureInfo.InvariantCulture) + "&img=" + img + "&alert=" + sound, string.Empty);
                    msgStatusLbl.Content = "Message sent successfully";
                }
                else
                {
                    msgStatusLbl.Content = "Incorrect value in count";
                }
            }
            catch (Exception e)
            {
                msgStatusLbl.Content = "Exception: " + e.Message;
            }
        }

        private void SendiOSMessage()
        {
            msgStatusLbl.Content = string.Empty;
            try
            {
                string msg = msgTxtBox.Text;
                string sound = soundTxtBox.Text;
                int count;
                if (int.TryParse(countTxtBox.Text, out count) || string.IsNullOrEmpty(countTxtBox.Text))
                {
                    string sub = this.GetSelectedSubscription();
                    SendPostRequest(BaseAddress + "/message/iOS/" + sub + "?mesg=" + msg + "&count=" + count.ToString(CultureInfo.InvariantCulture) + "&alert=" + sound, string.Empty);
                    msgStatusLbl.Content = "Message sent successfully";

                }
                else
                {
                    msgStatusLbl.Content = "Incorrect value in count";
                }
            }
            catch (Exception e)
            {
                msgStatusLbl.Content = "Exception: " + e.Message;
            }
        }

        private void SendRawMessage()
        {
            msgStatusLbl.Content = string.Empty;
            try
            {
                string msg = msgTxtBox.Text;
                string sub = this.GetSelectedSubscription();
                if (!string.IsNullOrEmpty(msg))
                {
                    SendPostRequest(BaseAddress + "/message/raw/" + sub + "?mesg=" + msg, string.Empty);
                    msgStatusLbl.Content = "Message sent successfully";
                }
                else
                {
                    msgStatusLbl.Content = "Message should not be blank";
                }
            }
            catch (Exception e)
            {
                msgStatusLbl.Content = "Exception: " + e.Message;
            }
        }

        private void SendTileMessage()
        {
            msgStatusLbl.Content = string.Empty;
            try
            {
                string msg = msgTxtBox.Text;
                string img = imgTxtBox.Text;
                int count;
                if (int.TryParse(countTxtBox.Text, out count) || string.IsNullOrEmpty(countTxtBox.Text))
                {
                    string sub = this.GetSelectedSubscription();
                    SendPostRequest(BaseAddress + "/message/tile/" + sub + "?mesg=" + msg + "&count=" + count.ToString(CultureInfo.InvariantCulture) + "&img=" + img, string.Empty);
                    msgStatusLbl.Content = "Message sent successfully";
                }
                else
                {
                    msgStatusLbl.Content = "Incorrect value in count";
                }
            }
            catch (Exception e)
            {
                msgStatusLbl.Content = "Exception: " + e.Message;
            }
        }

        private void SendToastMessage()
        {
            msgStatusLbl.Content = string.Empty;
            try
            {
                string msg = msgTxtBox.Text;
                string sub = this.GetSelectedSubscription();
                if (!string.IsNullOrEmpty(msg))
                {
                    SendPostRequest(BaseAddress + "/message/toast/" + sub + "?mesg=" + msg, string.Empty);
                    msgStatusLbl.Content = "Message sent successfully";
                }
                else
                {
                    msgStatusLbl.Content = "Message should not be blank";
                }
            }
            catch (Exception e)
            {
                msgStatusLbl.Content = "Exception: " + e.Message;
            }
        }

        private string GetSelectedSubscription()
        {
            int index = subscriptionsComboBox.SelectedIndex;
            string sub = this.subscriptionsList[index];
            return sub;
        }

        private void TxtboxGotFocus(object sender, RoutedEventArgs e)
        {
            msgStatusLbl.Content = string.Empty;
            statusTextBlock.Text = string.Empty;
        }
    }
}
