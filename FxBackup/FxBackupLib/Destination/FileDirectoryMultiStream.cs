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

		#region MultiStream implementation
		protected override Stream CreateStreamImpl (Guid id)
		{			
			string fileName = Path.Combine (location, id.ToString ());
			return new FileStream (fileName, FileMode.CreateNew, FileAccess.Write, FileShare.None);
		}
		#endregion
	}
}

