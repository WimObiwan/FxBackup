using System;
using System.IO;


namespace FxBackupLib
{
	public interface IOriginItemStream
	{
		Stream GetStream();
	}
}

