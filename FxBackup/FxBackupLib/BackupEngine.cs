using System;
using System.Collections.Generic;
using System.IO;


namespace FxBackupLib
{
	public class BackupEngine
	{
		StreamPump streamPump = new StreamPump();
		public List<IOrigin> Origins { get; private set; }
		public Archive Archive { get; private set; }
		
		public BackupEngine (Archive archive)
		{
			Origins = new List<IOrigin> ();
			Archive = archive;
		}
		
		public void Run ()
		{
			foreach (IOrigin origin in Origins) {
				ProcessOrigin (origin);
			}
			Archive.WriteIndex ();
		}

		void ProcessOrigin (IOrigin origin)
		{
			IOriginItem originItem = origin.GetRootItem ();
			ArchiveItem archiveItem = Archive.CreateRootItem (originItem.Name);
			ProcessOriginItem (archiveItem, originItem);
		}

		void ProcessOriginItem (ArchiveItem archiveItem, IOriginItem originItem)
		{
			Console.WriteLine (originItem.Name);
			foreach (IOriginItemStream originItemStream in originItem.Streams) {
				using (Stream inputStream = originItemStream.GetStream()) {
					ArchiveStream archiveStream = archiveItem.CreateStream (originItemStream.Id);
					using (Stream outputStream = Archive.CreateStream(archiveStream)) {
						byte[] hash;
						streamPump.Copy (inputStream, outputStream, out hash);
						archiveStream.Hash = hash;
					}
				}
			}
			
			foreach (IOriginItem originSubItem in originItem.SubItems) {
				ArchiveItem archiveSubItem = archiveItem.CreateChildItem (originSubItem.Name);
				ProcessOriginItem (archiveSubItem, originSubItem);
			}
		}
	}
}

