using System;
using System.IO;


namespace FxBackupLib
{
	public interface IOriginItemStream
	{
		Guid Id { get; }
		Stream GetStream();
	}
}

