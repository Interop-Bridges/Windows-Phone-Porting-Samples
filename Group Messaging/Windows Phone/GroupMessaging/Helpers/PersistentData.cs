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
using GroupMessaging.Models;

namespace GroupMessaging.Helpers
{
	[DataContract]
	public class PersistentData : IBinarySerializable
	{
		[DataMember]
		public List<Group> Groups { get; set; }

		#region IBinarySerializable Members

		public void Write(System.IO.BinaryWriter writer)
		{
			// write the Groups
			if (Groups != null)
			{
				writer.Write(Groups.Count);
				foreach (var group in Groups)
				{
					group.Write(writer);
				}
			}
			else
			{
				writer.Write(0);
			}
		}

		public void Read(System.IO.BinaryReader reader)
		{
			// read the Groups
			Groups = new List<Group>();
			int groupsCount = reader.ReadInt32();
			if (groupsCount > 0)
			{
				for (int i = 0; i < groupsCount; ++i)
				{
					var group = new Group();
					group.Read(reader);
					Groups.Add(group);
				}
			}
		}

		#endregion
	}
}
