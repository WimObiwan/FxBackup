using System;
using System.IO;


namespace FxBackupLib
{
	public abstract class MultiStream
	{
		// Enforce serialized access (only one stream open)
		Stream lastStream;
		
		public Stream CreateStream (Guid id)
		{
			if (lastStream != null)
				lastStream.Dispose ();
			lastStream = CreateStreamImpl (id);
			return lastStream;
		}

		public Stream OpenStream (Guid id)
		{
			if (lastStream != null)
				lastStream.Dispose ();
			lastStream = OpenStreamImpl (id);
			return lastStream;
		}
		
		protected abstract Stream CreateStreamImpl(Guid id);
		protected abstract Stream OpenStreamImpl(Guid id);
	}
}

