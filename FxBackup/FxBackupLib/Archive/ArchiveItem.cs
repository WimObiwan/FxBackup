using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FxBackupLib
{
	public class ArchiveItem
	{
		Archive archive;

		internal ArchiveItem ()
		{
			ChildItems = new List<ArchiveItem> ();
		}
		
		internal ArchiveItem (Archive archive, string name, int type)
			: this()
		{
			this.archive = archive;
			Name = name;
			Type = type;
		}
		
		public string Name { get; private set; }
		public int Type { get; private set; }
		internal List<ArchiveItem> ChildItems;
		public Guid PhysicalStoreDataStreamId { get; private set; }
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

		public ArchiveItem CreateChildItem (string name, int type)
		{
			ArchiveItem subItem = new ArchiveItem (archive, name, type);
			ChildItems.Add (subItem);
			return subItem;
		}
		
		internal void Serialize (BinaryWriter writer)
		{
			writer.Write (Name);
			writer.Write (Type);
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

		public void Deserialize (Archive archive, BinaryReader reader)
		{
			this.archive = archive;
			Name = reader.ReadString ();
			Type = reader.ReadInt32 ();
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
				ArchiveItem subItem = new ArchiveItem ();
				ChildItems.Add (subItem);
				subItem.Deserialize (archive, reader);
			}
		}
	}
}

