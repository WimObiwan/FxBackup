using System;
using System.Collections.Generic;
using System.IO;


namespace FxBackupLib
{
	public class BackupEngine
	{
		BackupEngineStreamPump streamPump = new BackupEngineStreamPump();
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
			ProcessOriginItem (originItem);
		}

		void ProcessOriginItem (IOriginItem originItem)
		{
			Console.WriteLine (originItem.Name);
			foreach (IOriginItemStream originItemStream in originItem.Streams) {
				using (Stream inputStream = originItemStream.GetStream()) {
					using (Stream outputStream = ItemStore.CreateItem (originItem.Name, originItemStream.Id)) {
						streamPump.Pump (inputStream, outputStream);
					}
				}
			}
			
			foreach (IOriginItem subOriginItem in originItem.SubItems) {
				ProcessOriginItem (subOriginItem);
			}
		}
	}
}

