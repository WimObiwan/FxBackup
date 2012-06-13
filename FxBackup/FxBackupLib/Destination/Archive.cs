using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace FxBackupLib
{
	public class Archive
	{
		MultiStream physicalStore;
		readonly Guid IndexStreamId = new Guid ("26108f76-8a54-4387-83ba-0ef468517a3b");
			
		List<ArchiveItem> rootItems = new List<ArchiveItem> ();
		
		public IEnumerable<ArchiveItem> RootItems {
			get {
				return rootItems;
			}
		}
		
		public Archive (MultiStream physicalStore)
		{
			this.physicalStore = physicalStore;
		}
		
		public ArchiveItem CreateRootItem (string itemName, int type)
		{
			ArchiveItem item = new ArchiveItem (this, itemName, type);
			rootItems.Add (item);
			return item;	
		}
		
		internal Stream CreateStream (Guid id)
		{
			return physicalStore.CreateStream (id);
		}
		
		internal Stream OpenStream (Guid id)
		{
			return physicalStore.OpenStream (id);
		}
		
		public void WriteIndex ()
		{
			using (Stream stream = physicalStore.CreateStream (IndexStreamId)) {
				using (BinaryWriter writer = new BinaryWriter(stream)) {
					writer.Write (rootItems.Count);
					foreach (ArchiveItem item in rootItems) {
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
						ArchiveItem item = new ArchiveItem ();
						rootItems.Add (item);
						item.Deserialize (this, reader);
					}
				}
			}
		}

	}
}

