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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;

namespace DisplayingAlerts
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Triggered when the btnMessage control is clicked. Displays a message box to the user which will display a second confirmation
        /// messaged box if the user clicks "OK."
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMessage_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This is a Message Box", "Migration - Displaying Alerts", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                MessageBox.Show("Thank you for pressing ok.");
            }
        }

        /// <summary>
        /// Triggered when the btnXNAMessage control is clicked. Displays an asynchronous XNA message box to the user which poses a question
        /// and provides two button with custom responses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnXNAMessage_Click(object sender, RoutedEventArgs e)
        {
            Guide.BeginShowMessageBox("Migration - Displaying Alerts", "Star Trek or Star Wars?",
                new List<string> { "Star Trek", "Star Wars" }, 0, MessageBoxIcon.None,
                new AsyncCallback(OnXNAMessageClose), null);
        }

        /// <summary>
        /// Aynchronous callback function for the XNA message box called via btnXNAMessage_Click, triggered when the XNA message box closes.
        /// Sends a message box method call back to the main UI thread containing a message to the user based upon their XNA message box response.
        /// </summary>
        /// <param name="aRes">Result of user interaction with XNA message box</param>
        private void OnXNAMessageClose(IAsyncResult aRes)
        {
            switch (Guide.EndShowMessageBox(aRes))
            {
                case 0: //Star Trek
                    Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show("That's great!"));
                    break;
                case 1: //Star Wars
                    Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show("It's not my favourite."));
                    break;
                default: //Other (eg. pressing back)
                    Deployment.Current.Dispatcher.BeginInvoke(() => MessageBox.Show("What just happened?"));
                    break;
            }
        }
    }
}