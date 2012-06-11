using System;
using System.IO;


namespace FxBackupLib
{
	public class FileSystemOrigin : IOrigin
	{
		string rootItem;
			
		public FileSystemOrigin (string rootItem)
		{
			this.rootItem = rootItem;
		}

		#region IOrigin implementation
		public IOriginItem GetRootItem ()
		{
			FileSystemInfo fileSystemInfo;
			if (Directory.Exists (rootItem))
				fileSystemInfo = new DirectoryInfo (rootItem);
			else
				fileSystemInfo = new FileInfo (rootItem);
			return new FileSystemOriginItem (fileSystemInfo);
		}
		#endregion
	}
}

