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
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GroupMessaging.Helpers;
using GroupMessaging.Models;

namespace GroupMessaging.ViewModels
{
	public class GroupViewModel : BaseViewModel
	{
		private const string DEFAULT_GROUP_NAME = "New Group";

		private Group _group;

		private string _name;
		public string GroupName
		{
			get { return _name; }
			set { _name = value; RaisePropertyChanged("GroupName"); }
		}

		public ObservableCollection<Contact> Contacts { get; set; }

		public GroupViewModel()
		{
			Contacts = new ObservableCollection<Contact>();
		}

		public void LoadGroup(Guid id)
		{
			Contacts.Clear();

			if (AppState.Current.Groups.ContainsKey(id))
			{
				_group = AppState.Current.Groups[id];

				var orderedContacts = _group.Contacts.OrderBy(g => g.Name);
				foreach (var contact in orderedContacts)
				{
					Contacts.Add(contact);
				}

				GroupName = _group.Name;
			}
			else
			{
				// set a default name
				GroupName = DEFAULT_GROUP_NAME;

				// create the new group, but don't save till they take action on it
				_group = new Group
				{
					Id = Guid.NewGuid(),
					Name = GroupName,
					Contacts = new List<Contact>(),
				};

				// add group to list
				AppState.Current.Groups.Add(_group.Id, _group);
				_SaveGroup();

				// set the current group id
				AppState.Current.CurrentGroupId = _group.Id;
			}
		}

		public void AddContact(string name, string number)
		{
			// check if this person already exists as a contact
			bool alreadyExists = AppState.Current.Groups[_group.Id].Contacts
				.Any(c => c.Name == name && c.Number == number);
			if (alreadyExists)
			{
				// if so then immediately short-circuit and don't add again
				return;
			}

			// add the new contact
			AppState.Current.Groups[_group.Id].Contacts.Add(
				new Contact
				{
					Name = name,
					Number = number,
				});

			_SaveGroup();

			// NOTE: don't need to refresh the contact list as that is
			// handled by the app navigating back to the group page.
		}

		public void DeleteContact(Contact contact)
		{
			// remove the contact from the list
			var contactToDelete = AppState.Current.Groups[_group.Id].Contacts
				.SingleOrDefault(c => c.Name == contact.Name && c.Number == contact.Number);

			if (contactToDelete != null)
			{
				AppState.Current.Groups[_group.Id].Contacts.Remove(contactToDelete);
				_SaveGroup();

				// update the contacts list
				Contacts.Remove(contact);
			}
		}

		public void DeleteGroup()
		{
			// if the group exists in the list, remove it
			if (AppState.Current.Groups.ContainsKey(_group.Id))
			{
				AppState.Current.Groups.Remove(_group.Id);
			}

			// make the call now to save off to isolated storage
			AppState.Current.SavePersistentData();
		}

		public void SaveUpdatedName()
		{
			// update the name
			_group.Name = GroupName;

			_SaveGroup();
		}

		private void _SaveGroup()
		{
			// make the call now to save off to isolated storage
			AppState.Current.SavePersistentData();
		}
	}
}