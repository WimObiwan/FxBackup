using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FxBackupLib
{
	public partial class Archive
	{
		public class Item
		{
			Archive itemStore;

			/*internal struct Tuple<T1, T2>
			{
				public readonly T1 Item1;
				public readonly T2 Item2;
	
				public Tuple (T1 item1, T2 item2)
				{
					Item1 = item1;
					Item2 = item2;
				} 
			}
			
			internal static class Tuple
			{ // for type-inference goodness.
				public static Tuple<T1,T2> Create<T1,T2> (T1 item1, T2 item2)
				{ 
					return new Tuple<T1,T2> (item1, item2); 
				}
			}*/
			
			internal Item (Archive itemStore, string name)
			{
				this.itemStore = itemStore;
				Name = name;
				ChildItems = new List<Item> ();
				Streams = new List<Tuple<Guid, Guid>> ();
			}
			
			public string Name { get; private set; }

			internal List<Item> ChildItems;
			internal List<Tuple<Guid, Guid>> Streams;
			
			public Stream CreateStream (Guid streamId)
			{
				Guid id = Guid.NewGuid ();
				Streams.Add (Tuple.Create (streamId, id));
				return itemStore.CreateStream (id);
			}

			public Stream OpenStream (Guid streamId)
			{
				return itemStore.OpenStream (Streams.Single (p => p.Item1 == streamId).Item2);
			}

			public Archive.Item CreateChildItem (string name)
			{
				Item subItem = new Item (itemStore, name);
				ChildItems.Add (subItem);
				return subItem;
			}
			
			internal void Serialize (BinaryWriter writer)
			{
				writer.Write (Name);
				writer.Write (ChildItems.Count);
				foreach (Item item in ChildItems) {
					item.Serialize (writer);
				}
				writer.Write (Streams.Count);
				
				foreach (var streamItem in Streams) {
					writer.Write (streamItem.Item1.ToByteArray ());
					writer.Write (streamItem.Item2.ToByteArray ()); 
				}
			}

			public void Deserialize (BinaryReader reader)
			{
				Name = reader.ReadString ();
				int cnt = reader.ReadInt32 ();
				while (cnt-- > 0) {
					Item subItem = new Item (itemStore, null);
					ChildItems.Add (subItem);
					subItem.Deserialize (reader);
				}
				cnt = reader.ReadInt32 ();
				while (cnt-- > 0) {
					byte[] by = new byte[16];
					reader.Read (by, 0, 16);
					Guid g1 = new Guid (by);
					reader.Read (by, 0, 16);
					Guid g2 = new Guid (by);
					Streams.Add (Tuple.Create (g1, g2));
				}
			}
		}		
	}
}

