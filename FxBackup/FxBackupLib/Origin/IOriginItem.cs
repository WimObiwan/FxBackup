using System;
using System.Collections.Generic;


namespace FxBackupLib
{
	public interface IOriginItem
	{
		string Name { get; }
		string Path { get; }
		System.Collections.Generic.IEnumerable<IOriginItem> SubItems { get; }
		System.Collections.Generic.IEnumerable<IOriginItemStream> Streams { get; }
	}
}
