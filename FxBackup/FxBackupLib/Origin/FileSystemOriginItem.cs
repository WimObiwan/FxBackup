using System;
using System.IO;

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

		public System.Collections.Generic.IEnumerable<IOriginItem> SubItems {
			get {
				DirectoryInfo directoryInfo = FileSystemInfo as DirectoryInfo;
				if (directoryInfo != null) {
					var enumerator = directoryInfo.EnumerateFileSystemInfos ().GetEnumerator ();
					while (enumerator.MoveNext ())
						yield return new FileSystemOriginItem (enumerator.Current);
				}
			}
		}
		
		public System.Collections.Generic.IEnumerable<IOriginItemStream> Streams {
			get {
				if ((FileSystemInfo.Attributes & FileAttributes.Directory) == 0)
					yield return new FileSystemOriginItemDataStream (FileSystemInfo);
				else
					yield break;
			}
		}

	}
}

