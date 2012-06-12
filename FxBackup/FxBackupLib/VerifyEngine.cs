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
		
		public VerifyEngine (Archive archive)
		{
			Origins = new List<IOrigin> ();
			Archive = archive;
		}
		
		public bool Run (bool hashOnly)
		{
			bool same = true;
			
			Archive.ReadIndex ();
			rootItems = Archive.RootItems.ToList ();
			foreach (IOrigin origin in Origins) {
				if (!ProcessOrigin (origin, hashOnly))
					same = false;
			}
			
			foreach (ArchiveItem item in rootItems) {
				Console.WriteLine ("Only present in Archive: {0}", item.Name);
			}
			
			return same;
		}

		bool ProcessOrigin (IOrigin origin, bool hashOnly)
		{
			bool same = true;
			
			IOriginItem originItem = origin.GetRootItem ();
			var item = rootItems.SingleOrDefault (p => p.Name == originItem.Name);
			if (item != null) {
				rootItems.Remove (item);
				same = ProcessOriginItem (item, originItem, hashOnly);
			} else {
				Console.WriteLine ("Only present in Origin: {0}", originItem.Name);
			}
			
			return same;
		}

		bool ProcessOriginItem (ArchiveItem archiveItem, IOriginItem originItem, bool hashOnly)
		{
			bool same = true;
			
			Console.WriteLine ("Verifying {0}", originItem.Name);
			
			var streams = archiveItem.Streams.ToList ();			
			foreach (IOriginItemStream originItemStream in originItem.Streams) {
				ArchiveStream archiveStream = streams.SingleOrDefault (p => p.StreamId == originItemStream.Id);
				if (archiveStream != null) {
					streams.Remove (archiveStream);
					using (Stream inputStream = originItemStream.GetStream()) {
						if (hashOnly) {
							if (!streamVerifier.Verify (inputStream, archiveStream.Hash))
								same = false;
						} else {
							using (Stream outputStream = Archive.OpenStream(archiveStream)) {
								if (!streamVerifier.Verify (inputStream, outputStream))
									same = false;
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
					if (!ProcessOriginItem (item, subOriginItem, hashOnly))
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
	}
}

