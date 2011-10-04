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
using System.Net;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GroupMessaging.Helpers;

namespace GroupMessaging.Models
{
	[DataContract]
	public class Group : IBinarySerializable
	{
		[DataMember]
		public Guid Id { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public List<Contact> Contacts { get; set; }

		#region IBinarySerializable Members

		public void Write(System.IO.BinaryWriter writer)
		{
			// write the Id in string format
			writer.Write(Id.ToString());

			// write the name
			writer.Write(Name);

			// write the Contacts
			if (Contacts != null)
			{
				writer.Write(Contacts.Count);
				foreach (var contact in Contacts)
				{
					contact.Write(writer);
				}
			}
			else
			{
				writer.Write(0);
			}
		}

		public void Read(System.IO.BinaryReader reader)
		{
			// read the Id from string format
			string guid = reader.ReadString();
			Guid parsedId;
			if (Guid.TryParse(guid, out parsedId))
			{
				Id = parsedId;
			}
			else
			{
				Id = Guid.NewGuid();
			}

			// read the name
			Name = reader.ReadString();

			// read the Contacts
			Contacts = new List<Contact>();
			int contactsCount = reader.ReadInt32();
			if (contactsCount > 0)
			{
				for (int i = 0; i < contactsCount; ++i)
				{
					var contact = new Contact();
					contact.Read(reader);
					Contacts.Add(contact);
				}
			}
		}

		#endregion
	}
}
