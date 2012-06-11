using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace FxBackupLib
{
	public partial class Archive
	{
		MultiStream physicalStore;
		readonly Guid IndexStreamId = new Guid ("26108f76-8a54-4387-83ba-0ef468517a3b");
			
		List<Item> rootItems = new List<Item> ();
		
		public IEnumerable<Item> RootItems {
			get {
				return rootItems;
			}
		}
		
		public Archive (MultiStream physicalStore)
		{
			this.physicalStore = physicalStore;
		}
		
		public Item CreateRootItem (string itemName)
		{
			Item item = new Item (this, itemName);
			rootItems.Add (item);
			return item;	
		}
		
		private Stream CreateStream (Guid id)
		{
			return physicalStore.CreateStream (id);
		}
		
		private Stream OpenStream (Guid id)
		{
			return physicalStore.OpenStream (id);
		}
		
		public void WriteIndex ()
		{
			using (Stream stream = physicalStore.CreateStream (IndexStreamId)) {
				using (BinaryWriter writer = new BinaryWriter(stream)) {
					writer.Write (rootItems.Count);
					foreach (Item item in rootItems) {
						item.Serialize (writer);
					}
				}
			}
		}

		public void ReadIndex ()
		{
			using (Stream stream = physicalStore.OpenStream (IndexStreamId)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					int count = reader.ReadInt32 ();
					while (count-- > 0) {
						Item item = new Item (this, null);
						rootItems.Add (item);
						item.Deserialize (reader);
					}
				}
			}
		}

	}
}

