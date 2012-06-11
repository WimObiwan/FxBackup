using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FxBackupLib
{
	public class VerifyEngine
	{
		StreamVerifier streamVerifier = new StreamVerifier();
		public List<IOrigin> Origins { get; private set; }
		public Archive ItemStore { get; private set; }
		
		List<Archive.Item> rootItems;
		
		public VerifyEngine (Archive itemStore)
		{
			Origins = new List<IOrigin> ();
			ItemStore = itemStore;
		}
		
		public bool Run ()
		{
			bool same = true;
			
			ItemStore.ReadIndex ();
			rootItems = ItemStore.RootItems.ToList ();
			foreach (IOrigin origin in Origins) {
				if (!ProcessOrigin (origin))
					same = false;
			}
			
			foreach (Archive.Item item in rootItems) {
				Console.WriteLine ("Only present in ItemStore: {0}", item.Name);
			}
			
			return same;
		}

		bool ProcessOrigin (IOrigin origin)
		{
			bool same = true;
			
			IOriginItem originItem = origin.GetRootItem ();
			var item = rootItems.SingleOrDefault (p => p.Name == originItem.Name);
			if (item != null) {
				rootItems.Remove (item);
				same = ProcessOriginItem (item, originItem);
			} else {
				Console.WriteLine ("Only present in Origin: {0}", originItem.Name);
			}
			
			return same;
		}

		bool ProcessOriginItem (Archive.Item itemStoreItem, IOriginItem originItem)
		{
			bool same = true;
			
			Console.WriteLine ("Verifying {0}", originItem.Name);
			
			var streams = itemStoreItem.Streams.ToList ();			
			foreach (IOriginItemStream originItemStream in originItem.Streams) {
				Tuple<Guid, Guid> item = streams.SingleOrDefault (p => p.Item1 == originItemStream.Id);
				if (item != null) {
					streams.Remove (item);
					using (Stream inputStream = originItemStream.GetStream()) {
						using (Stream outputStream = itemStoreItem.OpenStream (originItemStream.Id)) {
							if (!streamVerifier.Verify (inputStream, outputStream))
								same = false;
						}
					}
				} else {
					Console.WriteLine ("Only present in Origin: {0}", originItemStream.Id);
					same = false;
				}
			}
			
			foreach (var stream in streams) {
				Console.WriteLine ("Only present in ItemStore: {0}", stream.Item1);
				same = false;
			}
			
			var childItems = itemStoreItem.ChildItems.ToList ();			
			foreach (IOriginItem subOriginItem in originItem.SubItems) {
				var item = childItems.SingleOrDefault (p => p.Name == subOriginItem.Name);
				if (item != null) {
					childItems.Remove (item);
					if (!ProcessOriginItem (item, subOriginItem))
						same = false;
				} else {
					Console.WriteLine ("Only present in Origin: {0}", subOriginItem.Name);
					same = false;
				}
			}
			
			foreach (var childItem in childItems) {
				Console.WriteLine ("Only present in ItemStore: {0}", childItem.Name);
				same = false;
			}
			
			return same;
		}
	}
}

