using System;
using System.IO;


namespace FxBackupLib
{
	public interface IOriginItemStream
	{
		Guid Id { get; }
		string Name { get; }
		Stream GetStream();
	}
}

