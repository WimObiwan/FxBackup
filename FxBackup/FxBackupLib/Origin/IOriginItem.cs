using System;
using System.Collections.Generic;


namespace FxBackupLib
{
	public interface IOriginItem
	{
		string Name { get; }
		System.Collections.Generic.IEnumerable<IOriginItem> SubItems { get; }
		System.Collections.Generic.IEnumerable<IOriginItemStream> Streams { get; }
	}
}
