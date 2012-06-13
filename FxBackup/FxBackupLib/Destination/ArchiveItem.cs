using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FxBackupLib
{
	public class ArchiveItem
	{
		Archive archive;

		internal ArchiveItem (Archive archive, string name)
		{
			this.archive = archive;
			Name = name;
			ChildItems = new List<ArchiveItem> ();
		}
		
		public string Name { get; private set; }

		internal List<ArchiveItem> ChildItems;
		public Guid PhysicalStoreDataStreamId { get; set; }
		public byte[] DataStreamHash { get; set; }
		
		public Stream CreateDataStream ()
		{
			PhysicalStoreDataStreamId = Guid.NewGuid ();
			return archive.CreateStream (PhysicalStoreDataStreamId);
		}
		
		public Stream OpenDataStream ()
		{
			if (PhysicalStoreDataStreamId == Guid.Empty)
				return null;
			else
				return archive.OpenStream (PhysicalStoreDataStreamId);
		}

		public ArchiveItem CreateChildItem (string name)
		{
			ArchiveItem subItem = new ArchiveItem (archive, name);
			ChildItems.Add (subItem);
			return subItem;
		}
		
		internal void Serialize (BinaryWriter writer)
		{
			writer.Write (Name);
			writer.Write (PhysicalStoreDataStreamId.ToByteArray ()); 
			if (DataStreamHash != null) {
				writer.Write ((byte)DataStreamHash.Length);
				writer.Write (DataStreamHash);
			} else {
				writer.Write ((byte)0);
			}
			writer.Write (ChildItems.Count);
			foreach (ArchiveItem item in ChildItems) {
				item.Serialize (writer);
			}
		}

		public void Deserialize (BinaryReader reader)
		{
			Name = reader.ReadString ();
			byte[] by = new byte[16];
			reader.Read (by, 0, 16);
			PhysicalStoreDataStreamId = new Guid (by);
			int hashLen = reader.ReadByte ();
			if (hashLen > 0) {
				DataStreamHash = reader.ReadBytes (hashLen);
			} else {
				DataStreamHash = null;
			}
			int cnt = reader.ReadInt32 ();
			while (cnt-- > 0) {
				ArchiveItem subItem = new ArchiveItem (archive, null);
				ChildItems.Add (subItem);
				subItem.Deserialize (reader);
			}
		}
	}
}

