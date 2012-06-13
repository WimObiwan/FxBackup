using System;
using System.Collections.Generic;
using System.IO;

namespace FxBackupLib
{
	public class ArchiveProgressEventArgs : EventArgs
	{
		public State State { get; private set; }
		public string Path { get; private set; }
		public long Done;
		public long Total;
				
		public ArchiveProgressEventArgs (State state, string path)
			: this(state, path, 0, 0)
		{
		}
		
		public ArchiveProgressEventArgs (State state, string path, long done, long total)
		{
			State = state;
			Path = path;
			Done = done;
			Total = total;
		}
	}

}

