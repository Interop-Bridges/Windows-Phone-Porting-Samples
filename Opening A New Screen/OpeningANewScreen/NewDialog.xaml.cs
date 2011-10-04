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
    public partial class NewDialog : PhoneApplicationPage
    {
        //Constructor
        public NewDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Triggered when the btnDialog control is clicked. Sets the public MainPage variable retValue to equal txtDialog.Text
        /// and then triggers the equivalent of a back button press to return to the previous page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDialog_Click(object sender, RoutedEventArgs e)
        {
            MainPage.retValue = txtDialog.Text;
            NavigationService.GoBack();
        }
    }
}