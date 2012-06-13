using System;
using System.IO;
using System.Collections.Generic;

namespace FxBackupLib
{
	public class FileSystemOriginItem : IOriginItem
	{
		public FileSystemInfo FileSystemInfo { get; private set; }
		
		public FileSystemOriginItem (FileSystemInfo fileSystemInfo)
		{
			this.FileSystemInfo = fileSystemInfo;
		}
		
		public string Name { get { return FileSystemInfo.Name; } }
		public string Path { get { return FileSystemInfo.FullName; } }

		public Stream OpenStream()
		{
			if ((FileSystemInfo.Attributes & FileAttributes.Directory) == 0)
				return new FileStream (FileSystemInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			else
				return null;
		}

		public IEnumerable<IOriginItem> SubItems {
			get {
				DirectoryInfo directoryInfo = FileSystemInfo as DirectoryInfo;
				if (directoryInfo != null) {
					var enumerator = directoryInfo.EnumerateFileSystemInfos ().GetEnumerator ();
					while (enumerator.MoveNext ())
						yield return new FileSystemOriginItem (enumerator.Current);
				}
			}
		}
	}
}

