using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace FxBackupLib
{
	public class ItemStore
	{
		MultiStream physicalStore;
		
		readonly Guid IndexStreamId = new Guid("26108f76-8a54-4387-83ba-0ef468517a3b");
		
		[Serializable]
		public struct Tuple<T1, T2> {
		    public readonly T1 Item1;
		    public readonly T2 Item2;
		    public Tuple(T1 item1, T2 item2) { Item1 = item1; Item2 = item2;} 
		}
		
		public static class Tuple { // for type-inference goodness.
		    public static Tuple<T1,T2> Create<T1,T2>(T1 item1, T2 item2) { 
		        return new Tuple<T1,T2>(item1, item2); 
		    }
		}
		
		readonly SerializableDictionary<Tuple<string, Guid>, Guid> index = new SerializableDictionary<Tuple<string, Guid>, Guid>();
		
		public ItemStore (MultiStream physicalStore)
		{
			this.physicalStore = physicalStore;
		}
		
		public Stream CreateItem (string itemName, Guid streamId)
		{
			Guid id = Guid.NewGuid ();
			index.Add (Tuple.Create (itemName, streamId), id);
			return physicalStore.CreateStream (id);
		}
		
		public void WriteIndex ()
		{
			using (Stream stream = physicalStore.CreateStream (IndexStreamId)) {
				XmlSerializer SerializerObj = new XmlSerializer (
					typeof(SerializableDictionary<Tuple<string, Guid>, Guid>), new Type[] {typeof(Tuple<string, Guid>)});
 				SerializerObj.Serialize (stream, index);
 			}
		}
	}
}

