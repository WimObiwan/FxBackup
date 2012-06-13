using System;
using System.Collections.Generic;
using System.IO;

namespace FxBackupLib
{
	public enum State { BeginItem, BeginStream, Block, EndStream, BeginChildItems, EndChildItems, EndItem };

	public class OriginProgressEventArgs : EventArgs
	{
		public State State { get; private set; }
		public IOriginItem OriginItem { get; private set; }
		public long Done;
		public long Total;
				
		public OriginProgressEventArgs (State state, IOriginItem originItem)
			: this(state, originItem, 0, 0)
		{
		}
		
		public OriginProgressEventArgs (State state, IOriginItem originItem, long done, long total)
		{
			State = state;
			OriginItem = originItem;
			Done = done;
			Total = total;
		}
	}

}

