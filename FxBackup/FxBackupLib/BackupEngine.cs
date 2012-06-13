using System;
using System.Collections.Generic;
using System.IO;

namespace FxBackupLib
{
	public class BackupEngine
	{
		protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger (typeof(BackupEngine));
		
		public List<IOrigin> Origins { get; private set; }

		public Archive Archive { get; private set; }		
		public delegate void ProgressEventHandler (object sender,ProgressEventArgs arg);

		public event ProgressEventHandler Progress;

		StreamPump streamPump;
		
		public BackupEngine (Archive archive)
		{
			Origins = new List<IOrigin> ();
			Archive = archive;
		}
		
		public void Run ()
		{
			logger.Info ("Starting Backup");
			streamPump = new StreamPump ();
			foreach (IOrigin origin in Origins) {
				ProcessOrigin (origin);
			}
			Archive.WriteIndex ();
			streamPump = null;
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
			
			if (Progress != null)
				Progress (this, new ProgressEventArgs (State.BeginItem, originItem));
			
			ProgressOriginItemStreams (archiveItem, originItem);
			ProgressOriginChildItems (archiveItem, originItem);
						
			if (Progress != null)
				Progress (this, new ProgressEventArgs (State.EndItem, originItem));
		}

		void ProgressOriginItemStreams (ArchiveItem archiveItem, IOriginItem originItem)
		{
			bool firstDone = false;
			foreach (IOriginItemStream originItemStream in originItem.Streams) {
				if (!firstDone) {
					if (Progress != null)
						Progress (this, new ProgressEventArgs (State.BeginStreams, originItem));
					firstDone = true;
				}
				using (Stream inputStream = originItemStream.GetStream()) {
					if (Progress != null)
						Progress (
							this,
							new ProgressEventArgs (State.BeginStream, originItem, originItemStream)
						);
					ArchiveStream archiveStream = archiveItem.CreateStream (originItemStream.Id);
					using (Stream outputStream = Archive.CreateStream(archiveStream)) {
						byte[] hash;
						streamPump.Progress = delegate(long done, long total) {
							if (Progress != null)
								Progress (
									this,
									new ProgressEventArgs (State.Block, originItem, originItemStream, done, total)
								);
						};
						streamPump.Copy (inputStream, outputStream, out hash);
						archiveStream.Hash = hash;
					}
				}
				if (Progress != null)
					Progress (
						this,
						new ProgressEventArgs (State.EndStream, originItem, originItemStream)
					);
			}
			if (!firstDone) {
				if (Progress != null)
					Progress (this, new ProgressEventArgs (State.EndStreams, originItem));
			}
		}

		void ProgressOriginChildItems (ArchiveItem archiveItem, IOriginItem originItem)
		{
			bool firstDone = false;
			foreach (IOriginItem originSubItem in originItem.SubItems) {
				if (!firstDone) {
					if (Progress != null)
						Progress (
							this,
							new ProgressEventArgs (State.BeginChildItems, originItem)
						);
					firstDone = true;
				}
				ArchiveItem archiveSubItem = archiveItem.CreateChildItem (originSubItem.Name);
				ProcessOriginItem (archiveSubItem, originSubItem);
			}
			if (!firstDone) {
				if (Progress != null)
					Progress (this, new ProgressEventArgs (State.EndChildItems, originItem));
			}
		}
	}
}

