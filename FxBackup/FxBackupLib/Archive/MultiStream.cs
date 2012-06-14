using System;
using System.IO;


namespace FxBackupLib
{
	public abstract class MultiStream : IDisposable
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

		public bool Exists (Guid id)
		{
			return ExistsImpl (id);
		}
		
		public void Close ()
		{
			CloseImpl ();
		}
		
		#region IDisposable implementation
		public void Dispose ()
		{
			if (lastStream != null) {
				lastStream.Dispose ();
				lastStream = null;
			}
			Close ();
		}
		#endregion		
		
		protected abstract Stream CreateStreamImpl(Guid id);
		protected abstract Stream OpenStreamImpl(Guid id);
		protected abstract bool ExistsImpl(Guid id);
		protected abstract void CloseImpl();
	}
}

