using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FxBackupLib
{
	public class VerifyEngine
	{
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
					Console.WriteLine ("Only present in Archive: {0}", item.Name);
					same = false;
				}
			}
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
				Console.WriteLine ("Only present in Origin: {0}", originItem.Name);
				same = false;
			}
			
			return same;
		}

		bool ProcessOriginItem (ArchiveItem archiveItem, IOriginItem originItem, VerificationType verificationType)
		{
			bool same = true;
			
			Console.WriteLine ("Verifying {0}", originItem.Name);
			
			var streams = archiveItem.Streams.ToList ();			
			foreach (IOriginItemStream originItemStream in originItem.Streams) {
				ArchiveStream archiveStream = streams.SingleOrDefault (p => p.StreamId == originItemStream.Id);
				if (archiveStream != null) {
					streams.Remove (archiveStream);
					if (verificationType == VerificationType.ArchiveHashWithArchiveData) {
						using (Stream outputStream = Archive.OpenStream(archiveStream)) {
							if (!streamVerifier.Verify (outputStream, archiveStream.Hash))
								same = false;
						}
					} else {
						using (Stream inputStream = originItemStream.GetStream()) {
							if (verificationType == VerificationType.ArchiveHashWithOriginData) {
								if (!streamVerifier.Verify (inputStream, archiveStream.Hash))
									same = false;
							} else if (verificationType == VerificationType.ArchiveDataWithOriginData) {
								using (Stream outputStream = Archive.OpenStream(archiveStream)) {
									if (!streamVerifier.Verify (inputStream, outputStream))
										same = false;
								}
							}
						}
					}
				} else {
					Console.WriteLine ("Only present in Origin: {0}", originItemStream.Id);
					same = false;
				}
			}
			
			foreach (var stream in streams) {
				Console.WriteLine ("Only present in Archive: {0}", stream.StreamId);
				same = false;
			}
			
			var childItems = archiveItem.ChildItems.ToList ();			
			foreach (IOriginItem subOriginItem in originItem.SubItems) {
				var item = childItems.SingleOrDefault (p => p.Name == subOriginItem.Name);
				if (item != null) {
					childItems.Remove (item);
					if (!ProcessOriginItem (item, subOriginItem, verificationType))
						same = false;
				} else {
					Console.WriteLine ("Only present in Origin: {0}", subOriginItem.Name);
					same = false;
				}
			}
			
			foreach (var childItem in childItems) {
				Console.WriteLine ("Only present in Archive: {0}", childItem.Name);
				same = false;
			}
			
			return same;
		}

		bool ProcessArchiveItem (ArchiveItem archiveItem)
		{
			bool same = true;
			
			foreach (ArchiveStream archiveStream in archiveItem.Streams) {
				using (Stream outputStream = Archive.OpenStream(archiveStream)) {
					if (!streamVerifier.Verify (outputStream, archiveStream.Hash))
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

