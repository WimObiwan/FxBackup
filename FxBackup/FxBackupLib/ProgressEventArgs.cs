using System;
using System.Collections.Generic;
using System.IO;

namespace FxBackupLib
{
	public enum State { BeginItem, BeginStreams, BeginStream, Block, EndStream, EndStreams, BeginChildItems, EndChildItems, EndItem };

	public class ProgressEventArgs : EventArgs
	{
		public State State { get; private set; }
		public IOriginItem OriginItem { get; private set; }
		public IOriginItemStream OriginItemStream { get; private set; }
		public long Done;
		public long Total;
				
		public ProgressEventArgs (State state, IOriginItem originItem)
			: this(state, originItem, null, 0, 0)
		{
		}
		
		public ProgressEventArgs (State state, IOriginItem originItem, IOriginItemStream originItemStream)
			: this(state, originItem, originItemStream, 0, 0)
		{
		}
		
		public ProgressEventArgs (State state, IOriginItem originItem, IOriginItemStream originItemStream, long done, long total)
		{
			State = state;
			OriginItem = originItem;
			OriginItemStream = originItemStream;
			Done = done;
			Total = total;
		}
	}

}

