using System;
using System.Collections.Generic;
using System.IO;


namespace FxBackupLib
{
	public class BackupEngine
	{
		BackupEngineStreamPump streamPump = new BackupEngineStreamPump();
		public List<IOrigin> Origins { get; private set; }
		
		public BackupEngine ()
		{
			Origins = new List<IOrigin> ();
		}
		
		public void Run()
		{
			foreach (IOrigin origin in Origins) {
				ProcessOrigin(origin);
			}
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
				ProcessOriginItemStream (originItemStream);
			}
			
			foreach (IOriginItem subOriginItem in originItem.SubItems) {
				ProcessOriginItem (subOriginItem);
			}
		}

		void ProcessOriginItemStream (IOriginItemStream originItemStream)
		{
			using (Stream inputStream = originItemStream.GetStream()) {
				streamPump.Pump (inputStream, null);
			}
		}
	}
}

