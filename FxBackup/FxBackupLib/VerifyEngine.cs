using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FxBackupLib
{
	public class VerifyEngine
	{
		protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger (typeof(VerifyEngine));
		StreamVerifier streamVerifier = new StreamVerifier ();

		public List<IOrigin> Origins { get; private set; }

		public Archive Archive { get; private set; }
		
		List<ArchiveItem> rootItems;
		
		public enum VerificationType
		{
			ArchiveHashWithOriginData,
			ArchiveDataWithOriginData,
			ArchiveHashWithArchiveData,
		}
		
		public VerifyEngine (Archive archive)
		{
			Origins = new List<IOrigin> ();
			Archive = archive;
		}
		
		public bool Run (VerificationType verificationType)
		{
			logger.Info ("Starting Verify");
			
			bool same = true;
			
			Archive.ReadIndex ();
			if (verificationType == VerificationType.ArchiveHashWithArchiveData) {
				foreach (ArchiveItem rootItem in Archive.RootItems) {
					if (!ProcessArchiveItem (rootItem))
						same = false;
				}
			} else {
				rootItems = Archive.RootItems.ToList ();
				foreach (IOrigin origin in Origins) {
					if (!ProcessOrigin (origin, verificationType))
						same = false;
				}
		
				foreach (ArchiveItem item in rootItems) {
					logger.WarnFormat ("Only present in Archive: {0}", item.Name);
					same = false;
				}
			}
			
			logger.Info ("Finished Verify");
			return same;
		}

		bool ProcessOrigin (IOrigin origin, VerificationType verificationType)
		{
			bool same = true;
			
			IOriginItem originItem = origin.GetRootItem ();
			var item = rootItems.SingleOrDefault (p => p.Name == originItem.Name);
			if (item != null) {
				rootItems.Remove (item);
				same = ProcessOriginItem (item, originItem, verificationType);
			} else {
				logger.WarnFormat ("Only present in origin: {0}", item.Name);
				same = false;
			}
			
			return same;
		}

		bool ProcessOriginItem (ArchiveItem archiveItem, IOriginItem originItem, VerificationType verificationType)
		{
			bool same = true;
			
			logger.InfoFormat ("Verifying {0}", originItem.Name);
			
			using (Stream inputStream = originItem.OpenStream()) {
				if (inputStream != null) {
					if (verificationType == VerificationType.ArchiveHashWithOriginData) {
						if (!streamVerifier.Verify (inputStream, archiveItem.DataStreamHash))
							same = false;
					} else if (verificationType == VerificationType.ArchiveDataWithOriginData) {
						using (Stream outputStream = archiveItem.OpenDataStream()) {
							if (outputStream != null) {
								if (!streamVerifier.Verify (inputStream, outputStream))
									same = false;
							} else {
								logger.WarnFormat ("Only present in Origin: {0}", originItem.Name);
								same = false;
							}
						}
					} else {
						throw new InvalidOperationException ();
					}
				} else {
					using (Stream outputStream = archiveItem.OpenDataStream()) {
						if (outputStream != null) {
							logger.WarnFormat ("Only present in Archive: {0}", archiveItem.Name);
							same = false;
						}
					}
				}
			}
			
			var childItems = archiveItem.ChildItems.ToList ();			
			foreach (IOriginItem subOriginItem in originItem.SubItems) {
				var item = childItems.SingleOrDefault (p => p.Name == subOriginItem.Name);
				if (item != null) {
					childItems.Remove (item);
					if (!ProcessOriginItem (item, subOriginItem, verificationType))
						same = false;
				} else {
					logger.WarnFormat ("Only present in Origin: {0}", subOriginItem.Name);
					same = false;
				}
			}
			
			foreach (var childItem in childItems) {
				logger.WarnFormat ("Only present in Archive: {0}", childItem.Name);
				same = false;
			}
			
			return same;
		}

		bool ProcessArchiveItem (ArchiveItem archiveItem)
		{
			bool same = true;
			
			using (Stream outputStream = archiveItem.OpenDataStream()) {
				if (outputStream != null) {
					if (!streamVerifier.Verify (outputStream, archiveItem.DataStreamHash))
						same = false;
				}
			}
			
			foreach (ArchiveItem archiveSubItem in archiveItem.ChildItems) {
				if (!ProcessArchiveItem (archiveSubItem))
					same = false;
			}
			
			return same;
		}

	}
}
