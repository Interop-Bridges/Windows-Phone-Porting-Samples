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
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace GroupMessaging.Views
{
	public partial class GroupView : PhoneApplicationPage
	{
		public GroupViewModel ViewModel
		{
			get { return DataContext as GroupViewModel; }
			set { DataContext = value; }
		}

		public GroupView()
		{
			InitializeComponent();

			ViewModel = new GroupViewModel();
		}

		protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			if (AppState.Current.CurrentGroupId == Guid.Empty)
			{
				string idKey = "id";
				Guid id;

				if (NavigationContext.QueryString.ContainsKey(idKey) &&
				    Guid.TryParse(NavigationContext.QueryString[idKey], out id))
				{
					AppState.Current.CurrentGroupId = id;
				}
				else
				{
					AppState.Current.CurrentGroupId = Guid.Empty;
				}
			}

			ViewModel.LoadGroup(AppState.Current.CurrentGroupId);

			CheckSmsFunctionality();
		}

		private void ApplicationBarAddContactButton_Click(object sender, EventArgs e)
		{
			// prompt for a contact
			var phoneNumberTask = new PhoneNumberChooserTask();
			phoneNumberTask.Completed += new EventHandler<PhoneNumberResult>(phoneNumberTask_Completed);
			phoneNumberTask.Show();
		}

		void phoneNumberTask_Completed(object sender, PhoneNumberResult e)
		{
			// after they select a contact, immediately save to isolated storage
			if (e.TaskResult == TaskResult.OK)
			{
				ViewModel.AddContact(e.DisplayName, e.PhoneNumber);
				CheckSmsFunctionality();
			}
		}

		private void ApplicationBarSendSmsButton_Click(object sender, EventArgs e)
		{
			var smsTask = new SmsComposeTask();
			smsTask.To = string.Join(";", ViewModel.Contacts.Select(c => c.Number));
			smsTask.Show();
		}

		private void ApplicationBarDeleteGroupMenu_Click(object sender, EventArgs e)
		{
			// confirm that they want to delete the group
			var result = MessageBox.Show("", "Delete Group?",  MessageBoxButton.OKCancel);
			if (result == MessageBoxResult.OK)
			{
				ViewModel.DeleteGroup();

				if (NavigationService.CanGoBack)
				{
					NavigationService.GoBack();
				}
				else
				{
					NavigationService.Navigate(new Uri("/Views/GroupListView.xaml", UriKind.Relative));
				}
			}
		}

		private void GroupName_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (ViewModel.GroupName != GroupName.Text)
			{
				ViewModel.GroupName = GroupName.Text;
				ViewModel.SaveUpdatedName();
			}
		}

		private void CheckSmsFunctionality()
		{
			int SMS_BUTTON_INDEX = 1;

			// enable the send sms button, but only if contacts are assigned
			if (ViewModel.Contacts.Count > 0)
			{
				((ApplicationBarIconButton)ApplicationBar.Buttons[SMS_BUTTON_INDEX]).IsEnabled = true;
			}
			else
			{
				((ApplicationBarIconButton)ApplicationBar.Buttons[SMS_BUTTON_INDEX]).IsEnabled = false;
			}
		}

		private void DeleteContactButton_Click(object sender, RoutedEventArgs e)
		{
			var contact = ((Button) sender).DataContext as Contact;

			if (contact != null)
			{
				string message = string.Format("{0}: {1}", contact.Name, contact.Number);
				var result = MessageBox.Show(message, "Delete Contact?", MessageBoxButton.OKCancel);

				if (result == MessageBoxResult.OK)
				{
					ViewModel.DeleteContact(contact);
					CheckSmsFunctionality();
				}
			}
		}
	}
}