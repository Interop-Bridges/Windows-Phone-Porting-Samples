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
using GroupMessaging.Helpers;
using GroupMessaging.Models;
using GroupMessaging.ViewModels;
using Microsoft.Phone.Controls;

namespace GroupMessaging.Views
{
	public partial class GroupListView : PhoneApplicationPage
	{
		public GroupListViewModel ViewModel
		{
			get { return DataContext as GroupListViewModel; }
			set { DataContext = value; }
		}

		public GroupListView()
		{
			InitializeComponent();

			ViewModel = new GroupListViewModel();
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			ViewModel.LoadGroups();

			AppState.Current.CurrentGroupId = Guid.Empty;
		}

		private void ApplicationBarAddGroupButton_Click(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/Views/GroupView.xaml", UriKind.Relative));
		}

		private void GroupList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// ignore scenarios when we navigate back to this page and clear what was previously selected
			if (GroupList.SelectedItem != null)
			{
				Guid id = ((Group) GroupList.SelectedItem).Id;
				var selectedGroupUri = string.Format("/Views/GroupView.xaml?id={0}", id);
				NavigationService.Navigate(new Uri(selectedGroupUri, UriKind.Relative));
			}
		}
	}
}