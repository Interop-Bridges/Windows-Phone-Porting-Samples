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
using System.IO;
using System.IO.IsolatedStorage;
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
using GroupMessaging.Models;
using Microsoft.Phone.Shell;

namespace GroupMessaging.Helpers
{
	public class AppState
	{
		private const string PERSISTENT_DATA_FILENAME = "persistent.dat";
		private const string CurrentGroupIdKey = "CurrentGroupId";

		public Dictionary<Guid, Group> Groups { get; set; }
		public Guid CurrentGroupId { get; set; }

		#region Singleton implementation

		private static AppState Instance = null;
		private static object lockObject = new object();

		private AppState()
		{
			Groups = new Dictionary<Guid, Group>();
		}

		public static AppState Current
		{
			get
			{
				if (Instance == null)
				{
					lock (lockObject)
					{
						if (Instance == null)
							Instance = new AppState();
					}
				}

				return Instance;
			}
		}

		public void Initialize()
		{
		}

		#endregion

		public void LoadPersistentData()
		{
			var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();

			// first check and see if the file exists
			if (!isolatedStorage.FileExists(PERSISTENT_DATA_FILENAME))
				return;

			try
			{
				using (var file = isolatedStorage.OpenFile(PERSISTENT_DATA_FILENAME, FileMode.Open, FileAccess.Read))
				{
					using (var reader = new BinaryReader(file))
					{
						var persistentData = reader.ReadGeneric<PersistentData>();

						Groups = persistentData.Groups.ToDictionary(g => g.Id);
					}
				}
			}
			catch (Exception)
			{
				// regardless what goes wrong here, delete the file
				// TODO: should probably alert the user that their data file was corrupted

				isolatedStorage.DeleteFile(PERSISTENT_DATA_FILENAME);
			}
		}

		public void SavePersistentData()
		{
			var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();

			var persistentData = new PersistentData
			{
				Groups = Groups.Values.ToList(),
			};

			using (var file = isolatedStorage.OpenFile(PERSISTENT_DATA_FILENAME, FileMode.Create, FileAccess.Write))
			{
				using (var writer = new BinaryWriter(file))
				{
					writer.Write(persistentData);
				}
			}
		}

		public void LoadTransientData()
		{
			if (PhoneApplicationService.Current.State.ContainsKey(CurrentGroupIdKey))
			{
				CurrentGroupId = (Guid) PhoneApplicationService.Current.State[CurrentGroupIdKey];
			}
		}

		public void SaveTransientData()
		{
			PhoneApplicationService.Current.State[CurrentGroupIdKey] = CurrentGroupId;
		}
	}
}
