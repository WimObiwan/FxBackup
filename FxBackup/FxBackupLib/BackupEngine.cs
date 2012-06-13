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
		
		public delegate void ProgressEventHandler (object sender,OriginProgressEventArgs arg);
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
			ArchiveItem archiveItem = Archive.CreateRootItem (Path.GetFileName (originItem.Name));
			ProcessOriginItem (archiveItem, originItem);
		}

		void ProcessOriginItem (ArchiveItem archiveItem, IOriginItem originItem)
		{			
			logger.DebugFormat ("Processing Origin {0}", originItem.Name);
			
			if (Progress != null)
				Progress (this, new OriginProgressEventArgs (State.BeginItem, originItem));
			
			ProcessOriginItemStreams (archiveItem, originItem);
			ProcessOriginChildItems (archiveItem, originItem);
						
			if (Progress != null)
				Progress (this, new OriginProgressEventArgs (State.EndItem, originItem));
		}

		void ProcessOriginItemStreams (ArchiveItem archiveItem, IOriginItem originItem)
		{
			using (Stream inputStream = originItem.OpenStream()) {
				if (inputStream != null) {					
					if (Progress != null)
						Progress (
							this,
							new OriginProgressEventArgs (State.BeginStream, originItem)
						);
					using (Stream outputStream = archiveItem.CreateDataStream()) {
						byte[] hash;
						streamPump.Progress = delegate(long done, long total) {
							if (Progress != null)
								Progress (
									this,
									new OriginProgressEventArgs (State.Block, originItem, done, total)
								);
						};
						streamPump.Copy (inputStream, outputStream, out hash);
						archiveItem.DataStreamHash = hash;
					}
					if (Progress != null)
						Progress (
							this,
							new OriginProgressEventArgs (State.EndStream, originItem)
						);
				}
			}
		}

		void ProcessOriginChildItems (ArchiveItem archiveItem, IOriginItem originItem)
		{
			bool firstDone = false;
			foreach (IOriginItem originSubItem in originItem.SubItems) {
				if (!firstDone) {
					if (Progress != null)
						Progress (
							this,
							new OriginProgressEventArgs (State.BeginChildItems, originItem)
						);
					firstDone = true;
				}
				ArchiveItem archiveSubItem = archiveItem.CreateChildItem (originSubItem.Name);
				ProcessOriginItem (archiveSubItem, originSubItem);
			}
			if (!firstDone) {
				if (Progress != null)
					Progress (this, new OriginProgressEventArgs (State.EndChildItems, originItem));
			}
		}
	}
}

