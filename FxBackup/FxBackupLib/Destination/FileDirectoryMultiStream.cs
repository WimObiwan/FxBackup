using System;
using System.IO;


namespace FxBackupLib
{
	public class FileDirectoryMultiStream : MultiStream
	{
		string location;
		
		public FileDirectoryMultiStream (string location)
		{
			this.location = location;
			if (!Directory.Exists (location))
				Directory.CreateDirectory (location);
		}
		
		private string GetFileName (Guid id)
		{
			return Path.Combine (location, id.ToString ());
		}

		#region MultiStream implementation
		protected override Stream CreateStreamImpl (Guid id)
		{			
			return new FileStream (GetFileName(id), FileMode.CreateNew, FileAccess.Write, FileShare.None);
		}
		
		protected override Stream OpenStreamImpl (Guid id)
		{
			return new FileStream (GetFileName(id), FileMode.Open, FileAccess.Read, FileShare.None);
		}
		#endregion
	}
}

