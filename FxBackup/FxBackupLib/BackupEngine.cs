using System;
using System.Collections.Generic;
using System.IO;


namespace FxBackupLib
{
	public class BackupEngine
	{
		protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger (typeof(BackupEngine));
		
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
			logger.Info ("Starting Backup");
			foreach (IOrigin origin in Origins) {
				ProcessOrigin (origin);
			}
			Archive.WriteIndex ();
			logger.Info ("Finished Backup");
		}

		void ProcessOrigin (IOrigin origin)
		{
			IOriginItem originItem = origin.GetRootItem ();
			logger.DebugFormat ("Processing Root Origin {0}", originItem.Name);
			ArchiveItem archiveItem = Archive.CreateRootItem (originItem.Name);
			ProcessOriginItem (archiveItem, originItem);
		}

		void ProcessOriginItem (ArchiveItem archiveItem, IOriginItem originItem)
		{
			logger.DebugFormat ("Processing Origin {0}", originItem.Name);
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

