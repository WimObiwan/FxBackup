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
		
		public const int Type_File = 1;
		public const int Type_Directory = 2;
		
		public int Type { 
			get { 
				if ((FileSystemInfo.Attributes & FileAttributes.Directory) != 0)
					return 2;
				else
					return 1;
			}
		}

		public Stream OpenStream ()
		{
			if ((FileSystemInfo.Attributes & FileAttributes.Directory) == 0)
				return new FileStream (
					FileSystemInfo.FullName,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite
				);
			else
				return null;
		}

		public Stream CreateStream ()
		{
			FileInfo fileInfo = FileSystemInfo as FileInfo;
			if (fileInfo == null)
				throw new InvalidOperationException ();
			
			return new FileStream (fileInfo.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None);
		}

		public IOriginItem CreateChildItem (string name, int type)
		{
			DirectoryInfo directoryInfo = FileSystemInfo as DirectoryInfo;
			if (directoryInfo == null)
				throw new InvalidOperationException ();
			
			string path = System.IO.Path.Combine (Path, name);
			
			FileSystemOriginItem item;
			switch (type) {
			case Type_File:
				FileSystemInfo info2 = new FileInfo (path);
				item = new FileSystemOriginItem(info2);
				break;
			case Type_Directory:
				FileSystemInfo info = Directory.CreateDirectory (path);
				item = new FileSystemOriginItem (info);
				break;
			default:
				throw new NotSupportedException ();
			}
			
			return item;
		}

		public IEnumerable<IOriginItem> ChildItems {
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

