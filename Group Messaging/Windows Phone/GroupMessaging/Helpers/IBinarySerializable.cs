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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace GroupMessaging.Helpers
{
	public interface IBinarySerializable
	{
		void Write(BinaryWriter writer);
		void Read(BinaryReader reader);
	}

	public static class BinarySerializerExtensions
	{
		public static void WriteList<T>(this BinaryWriter writer, List<T> list) where T : IBinarySerializable
		{
			if (list != null)
			{
				writer.Write(list.Count);
				foreach (T item in list)
				{
					item.Write(writer);
				}
			}
			else
			{
				writer.Write(0);
			}
		}

		public static void WriteList(this BinaryWriter writer, List<string> list)
		{
			if (list != null)
			{
				writer.Write(list.Count);
				foreach (string item in list)
				{
					writer.Write(item);
				}
			}
			else
			{
				writer.Write(0);
			}
		}

		public static void Write<T>(this BinaryWriter writer, T value) where T : IBinarySerializable
		{
			if (value != null)
			{
				writer.Write(true);
				value.Write(writer);
			}
			else
			{
				writer.Write(false);
			}
		}

		public static void Write(this BinaryWriter writer, DateTime value)
		{
			writer.Write(value.Ticks);
		}

		public static void WriteString(this BinaryWriter writer, string value)
		{
			writer.Write(value ?? string.Empty);
		}

		public static T ReadGeneric<T>(this BinaryReader reader) where T : IBinarySerializable, new()
		{
			if (reader.ReadBoolean())
			{
				T result = new T();
				result.Read(reader);
				return result;
			}
			return default(T);
		}

		public static List<string> ReadList(this BinaryReader reader)
		{
			int count = reader.ReadInt32();
			if (count > 0)
			{
				List<string> list = new List<string>();
				for (int i = 0; i < count; i++)
				{
					list.Add(reader.ReadString());
				}
				return list;
			}

			return null;
		}

		public static List<T> ReadList<T>(this BinaryReader reader) where T : IBinarySerializable, new()
		{
			int count = reader.ReadInt32();
			if (count > 0)
			{
				List<T> list = new List<T>();
				for (int i = 0; i < count; i++)
				{
					T item = new T();
					item.Read(reader);
					list.Add(item);
				}
				return list;
			}

			return null;
		}

		public static DateTime ReadDateTime(this BinaryReader reader)
		{
			var int64 = reader.ReadInt64();
			return new DateTime(int64);
		}
	}
}