using System;
using System.IO;


namespace FxBackupLib
{
	public class FileSystemOriginProvider : IOriginProvider
	{
		string directory;
		LimitedQueue<FileSystemInfo> queue;

		public FileSystemOriginProvider (string directory)
		{
			this.directory = directory;
		}
		
		public void Initialize ()
		{
			queue = new LimitedQueue<FileSystemInfo> ();
			ScanDirectory (directory);
		}
		
		public void Uninitialize ()
		{
			queue = null;
		}
		
		void ScanDirectory (string directory)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo (directory);
			foreach (FileSystemInfo info in directoryInfo.EnumerateFileSystemInfos ()) {
				queue.Enqueue (info);
				if ((info.Attributes & FileAttributes.Directory) != 0) {
					ScanDirectory (info.FullName);
				}
			}			
			queue.Enqueue (null);
		}
		
		#region IDisposable implementation
		public void Dispose ()
		{
			Uninitialize ();
		}
		#endregion

		#region IOriginProvider implementation
		public IOriginProviderItem GetNextItem ()
		{
			FileSystemInfo fileSystemInfo = queue.Dequeue ();
			if (fileSystemInfo == null)
				return null;
			FileSystemOriginProviderItem item = new FileSystemOriginProviderItem (fileSystemInfo);
			return item;
		}
		#endregion
	}
}

