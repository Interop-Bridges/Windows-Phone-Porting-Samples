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
	public class Contact : IBinarySerializable
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Number { get; set; }

		#region IBinarySerializable Members

		public void Write(System.IO.BinaryWriter writer)
		{
			// write the name
			writer.Write(Name);

			// write the number
			writer.Write(Number);
		}

		public void Read(System.IO.BinaryReader reader)
		{
			// read the name
			Name = reader.ReadString();

			// read the number
			Number = reader.ReadString();
		}

		#endregion
	}
}
