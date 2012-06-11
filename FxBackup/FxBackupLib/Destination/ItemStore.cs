using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;


namespace FxBackupLib
{
	public partial class ItemStore
	{
		MultiStream physicalStore;
		
		readonly Guid IndexStreamId = new Guid("26108f76-8a54-4387-83ba-0ef468517a3b");
		
		[Serializable]
		private struct Tuple<T1, T2> {
		    public readonly T1 Item1;
		    public readonly T2 Item2;
		    public Tuple(T1 item1, T2 item2) { Item1 = item1; Item2 = item2;} 
		}
		
		private static class Tuple { // for type-inference goodness.
		    public static Tuple<T1,T2> Create<T1,T2>(T1 item1, T2 item2) { 
		        return new Tuple<T1,T2>(item1, item2); 
		    }
		}
		
		List<Item> rootItems = new List<Item>();
		
		public ItemStore (MultiStream physicalStore)
		{
			this.physicalStore = physicalStore;
		}
		
		public Item CreateRootItem (string itemName)
		{
			Item item = new Item (this, itemName);
			rootItems.Add (item);
			return item;	
		}
		
		private Stream CreateStream(Guid id)
		{
			return physicalStore.CreateStream (id);
		}
		
		public void WriteIndex ()
		{
			using (Stream stream = physicalStore.CreateStream (IndexStreamId)) {
				foreach (Item item in rootItems) {
					item.Serialize (stream);
				}
			}
		}
	}
}

