using System;
using System.IO;


namespace FxBackupLib
{
	public class FileSystemOriginItemDataStream : IOriginItemStream
	{
		FileSystemInfo fileSystemInfo;
		
		public FileSystemOriginItemDataStream (FileSystemInfo fileSystemInfo)
		{
			this.fileSystemInfo = fileSystemInfo;
		}

		
		#region IOriginItemStream implementation
		public System.IO.Stream GetStream ()
		{
			return new FileStream (fileSystemInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}
		#endregion
	}
}

