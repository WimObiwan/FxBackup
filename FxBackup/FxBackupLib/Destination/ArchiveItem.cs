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
			Streams = new List<ArchiveStream> ();
		}
		
		public string Name { get; private set; }

		internal List<ArchiveItem> ChildItems;
		internal List<ArchiveStream> Streams;
		
		public ArchiveStream CreateStream (Guid streamId)
		{
			ArchiveStream archiveStream = new ArchiveStream (streamId);
			Streams.Add (archiveStream);
			return archiveStream;
		}
		
		public ArchiveStream GetStream (Guid streamId)
		{
			return Streams.Single (p => p.StreamId == streamId);
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
			writer.Write (ChildItems.Count);
			foreach (ArchiveItem item in ChildItems) {
				item.Serialize (writer);
			}
			writer.Write (Streams.Count);				
			foreach (var streamItem in Streams) {
				writer.Write (streamItem.StreamId.ToByteArray ());
				writer.Write (streamItem.PhysicalStoreStreamId.ToByteArray ()); 
				if (streamItem.Hash != null) {
					writer.Write ((byte)streamItem.Hash.Length);
					writer.Write (streamItem.Hash);
				} else {
					writer.Write ((byte)0);
				}
			}
		}

		public void Deserialize (BinaryReader reader)
		{
			Name = reader.ReadString ();
			int cnt = reader.ReadInt32 ();
			while (cnt-- > 0) {
				ArchiveItem subItem = new ArchiveItem (archive, null);
				ChildItems.Add (subItem);
				subItem.Deserialize (reader);
			}
			cnt = reader.ReadInt32 ();
			while (cnt-- > 0) {
				byte[] by = new byte[16];
				reader.Read (by, 0, 16);
				ArchiveStream archiveStream = new ArchiveStream (new Guid (by));
				reader.Read (by, 0, 16);
				archiveStream.PhysicalStoreStreamId = new Guid (by);
				int hashLen = reader.ReadByte ();
				archiveStream.Hash = reader.ReadBytes (hashLen);
				Streams.Add (archiveStream);
			}
		}
	}
}

