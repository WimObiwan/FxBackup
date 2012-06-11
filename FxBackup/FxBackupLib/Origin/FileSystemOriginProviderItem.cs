using System;
using System.IO;


namespace FxBackupLib
{
	public class FileSystemOriginProviderItem : IOriginProviderItem
	{
		FileSystemInfo fileSystemInfo;
		
		public FileSystemOriginProviderItem (FileSystemInfo fileSystemInfo)
		{
			this.fileSystemInfo = fileSystemInfo;
			
			currentStream = 0;
		}

		int currentStream;
		
		#region IOriginProviderItem implementation
		public OriginProviderItemStream GetNextStream ()
		{
			OriginProviderItemStream originProviderItemStream;
			switch (currentStream) {
			case 0:
				originProviderItemStream = new OriginProviderItemStream ("metadata", GetMetaDataStream ());
				break;
			case 1:
				originProviderItemStream = new OriginProviderItemStream ("data", GetDataStream ());
				break;
			case 2:
				originProviderItemStream = null;
				break;
			default :
				throw new InvalidOperationException ();
			}
			
			return originProviderItemStream;
		}
		#endregion

		Stream GetMetaDataStream ()
		{
			return new MemoryStream ();
		}

		Stream GetDataStream ()
		{
			return new FileStream(fileSystemInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}
	}
}

