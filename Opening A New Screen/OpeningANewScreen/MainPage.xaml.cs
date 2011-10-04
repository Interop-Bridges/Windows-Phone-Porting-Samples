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

namespace OpeningANewScreen
{
    public partial class MainPage : PhoneApplicationPage
    {
        //Integer value that will be retrieved by the newly opened screen
        public static int pubInteger = 325;

        //Default return value from the called dialog screen
        public static string retValue = "";

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Triggered when the btnNewScreen control is clicked. Navigates to NewScreen.xaml and uses a
        /// query string to pass the value of txtPassedValue.Text to the new screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNewScreen_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewScreen.xaml?passedVal=" + txtPassedValue.Text, UriKind.Relative));
        }

        /// <summary>
        /// Triggered when the btnShowDialog control is clicked. Navigates to NewDialog.xaml.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowDialog_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewDialog.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Triggered when MainPage.xaml is brought into focus. Sets the value of txbDiagResult.Text to equal the value
        /// of local public string retValue.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if(retValue!="")
                txbDiagResult.Text = retValue;
        }
    }
}