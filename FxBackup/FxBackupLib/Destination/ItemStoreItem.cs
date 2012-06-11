using System;
using System.Collections.Generic;
using System.IO;


namespace FxBackupLib
{
	public partial class ItemStore
	{
		public class Item
		{
			ItemStore itemStore;

			internal Item(ItemStore itemStore, string name)
			{
				this.itemStore = itemStore;
				Name = name;
				ChildItems = new List<Item>();
				Streams = new List<Tuple<Guid, Guid>>();
			}
			
			public string Name { get; private set; }			
			internal List<Item> ChildItems;
			private List<Tuple<Guid, Guid>> Streams;
			
			public Stream CreateStream (Guid streamId)
			{
				if (itemStore == null)
					throw new NotSupportedException ();
				Guid id = Guid.NewGuid ();
				Streams.Add (Tuple.Create(streamId, id));
				return itemStore.CreateStream (id);
			}

			public ItemStore.Item CreateSubItem (string name)
			{
				Item subItem = new Item (itemStore, name);
				ChildItems.Add (subItem);
				return subItem;
			}
			
			internal void Serialize (Stream stream)
			{
				BinaryWriter writer = new BinaryWriter (stream);
				writer.Write (Name);
				writer.Write (ChildItems.Count);
				foreach (Item item in ChildItems) {
					item.Serialize (stream);
				}
				writer.Write (Streams.Count);
				
				foreach (var streamItem in Streams) {
					writer.Write (streamItem.Item1.ToByteArray ());
					writer.Write (streamItem.Item2.ToByteArray ()); 
				}
			}
		}		
	}
}

