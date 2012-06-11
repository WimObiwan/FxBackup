using System;
using System.IO;


namespace FxBackupLib
{
	public class FileSystemOriginItemDataStream : IOriginItemStream
	{
		FileSystemInfo fileSystemInfo;
		readonly Guid StreamId = new Guid("d7368433-7540-470f-9e7e-812bf1a73497");
		
		public FileSystemOriginItemDataStream (FileSystemInfo fileSystemInfo)
		{
			this.fileSystemInfo = fileSystemInfo;
		}

		
		#region IOriginItemStream implementation
		public Guid Id {
			get {
				return StreamId;
			}
		}

		public System.IO.Stream GetStream ()
		{
			return new FileStream (fileSystemInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}
		#endregion
	}
}

