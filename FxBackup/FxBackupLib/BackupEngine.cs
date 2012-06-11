using System;
using System.Collections.Generic;
using System.IO;


namespace FxBackupLib
{
	public class BackupEngine
	{
		StreamPump streamPump = new StreamPump();
		public List<IOrigin> Origins { get; private set; }
		public Archive ItemStore { get; private set; }
		
		public BackupEngine (Archive itemStore)
		{
			Origins = new List<IOrigin> ();
			ItemStore = itemStore;
		}
		
		public void Run ()
		{
			foreach (IOrigin origin in Origins) {
				ProcessOrigin (origin);
			}
			ItemStore.WriteIndex ();
		}

		void ProcessOrigin (IOrigin origin)
		{
			IOriginItem originItem = origin.GetRootItem ();
			Archive.Item itemStoreItem = ItemStore.CreateRootItem (originItem.Name);
			ProcessOriginItem (itemStoreItem, originItem);
		}

		void ProcessOriginItem (Archive.Item itemStoreItem, IOriginItem originItem)
		{
			Console.WriteLine (originItem.Name);
			foreach (IOriginItemStream originItemStream in originItem.Streams) {
				using (Stream inputStream = originItemStream.GetStream()) {
					using (Stream outputStream = itemStoreItem.CreateStream (originItemStream.Id)) {
						streamPump.Copy (inputStream, outputStream);
					}
				}
			}
			
			foreach (IOriginItem subOriginItem in originItem.SubItems) {
				Archive.Item itemStoreSubItem = itemStoreItem.CreateChildItem (subOriginItem.Name);
				ProcessOriginItem (itemStoreSubItem, subOriginItem);
			}
		}
	}
}

