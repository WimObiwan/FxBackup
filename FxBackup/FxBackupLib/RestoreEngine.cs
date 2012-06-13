using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FxBackupLib
{
	public class RestoreEngine
	{
		protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger (typeof(VerifyEngine));
		StreamPump streamPump;

		public List<IOrigin> Origins { get; private set; }

		public IOrigin Origin { get; private set; }

		public Archive Archive { get; private set; }
		
		public delegate void ProgressEventHandler (object sender,OriginProgressEventArgs arg);

		public event ProgressEventHandler Progress;

		public enum VerificationType
		{
			ArchiveHashWithOriginData,
			ArchiveDataWithOriginData,
			ArchiveHashWithArchiveData,
		}
		
		public RestoreEngine (IOrigin origin, Archive archive)
		{
			Origin = origin;
			Archive = archive;
		}
		
		public void Run ()
		{
			logger.Info ("Starting Restore");
			
			streamPump = new StreamPump ();
			Archive.ReadIndex ();
			foreach (ArchiveItem archiveItem in Archive.RootItems) {
				ProcessArchiveItem (archiveItem, Origin.GetRootItem ());
			}
			streamPump = null;
			
			logger.Info ("Finished Restore");
		}

		void ProcessArchiveItem (ArchiveItem archiveItem, IOriginItem parentItem)
		{
			IOriginItem originItem = parentItem.CreateChildItem (
				archiveItem.Name,
				archiveItem.Type
			);
			
			if (Progress != null)
				Progress (this, new OriginProgressEventArgs (State.BeginItem, originItem));
			
			ProcessArchiveStreams (archiveItem, originItem);
			ProcessArchiveChildItems (archiveItem, originItem);

			if (Progress != null)
				Progress (this, new OriginProgressEventArgs (State.EndItem, originItem));
		}

		void ProcessArchiveStreams (ArchiveItem archiveItem, IOriginItem originItem)
		{
			using (Stream archiveStream = archiveItem.OpenDataStream()) {
				if (archiveStream != null) {
					if (Progress != null)
						Progress (
							this,
							new OriginProgressEventArgs (State.BeginStream, originItem)
						);
					
					using (Stream originStream = originItem.CreateStream ()) {
						byte[] hash;
						streamPump.Progress = delegate(long done, long total) {
							if (Progress != null)
								Progress (
									this,
									new OriginProgressEventArgs (State.Block, originItem, done, total)
								);
						};
						streamPump.Copy (archiveStream, originStream, out hash);
						//archiveStream.Hash = hash;
					}
				}
				if (Progress != null)
					Progress (
						this,
						new OriginProgressEventArgs (State.EndStream, originItem)
					);
			}
		}
		
		void ProcessArchiveChildItems (ArchiveItem archiveItem, IOriginItem originItem)
		{
			bool firstDone = false;
			foreach (ArchiveItem archiveSubItem in archiveItem.ChildItems) {
				if (!firstDone) {
					if (Progress != null)
						Progress (
							this,
							new OriginProgressEventArgs (State.BeginChildItems, originItem)
						);
					firstDone = true;
				}
				ProcessArchiveItem (archiveSubItem, originItem);
			}
			if (!firstDone) {
				if (Progress != null)
					Progress (
						this,
						new OriginProgressEventArgs (State.EndChildItems, originItem)
					);
			}
		}
	}
}