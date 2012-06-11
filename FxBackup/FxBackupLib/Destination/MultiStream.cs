using System;
using System.IO;


namespace FxBackupLib
{
	public abstract class MultiStream
	{
		Stream lastStream;
		
		public Stream CreateStream (Guid id)
		{
			// Enforce serialized access (only one stream open)
			if (lastStream != null)
				lastStream.Dispose ();
			lastStream = CreateStreamImpl (id);
			return lastStream;
		}
		
		protected abstract Stream CreateStreamImpl(Guid id);
	}
}

