using System;
using System.Collections.Generic;
using System.IO;


namespace FxBackupLib
{
	public class BackupEngine
	{
		StreamPump streamPump = new StreamPump();
		public List<IOrigin> Origins { get; private set; }
		public ItemStore ItemStore { get; private set; }
		
		public BackupEngine (ItemStore itemStore)
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
			ItemStore.Item itemStoreItem = ItemStore.CreateRootItem (originItem.Name);
			ProcessOriginItem (itemStoreItem, originItem);
		}

		void ProcessOriginItem (ItemStore.Item itemStoreItem, IOriginItem originItem)
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
				ItemStore.Item itemStoreSubItem = itemStoreItem.CreateChildItem (subOriginItem.Name);
				ProcessOriginItem (itemStoreSubItem, subOriginItem);
			}
		}
	}
}

