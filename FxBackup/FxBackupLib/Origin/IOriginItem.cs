using System;
using System.Collections.Generic;
using System.IO;


namespace FxBackupLib
{
	public interface IOriginItem
	{
		string Name { get; }
		string Path { get; }
		Stream OpenStream();
		System.Collections.Generic.IEnumerable<IOriginItem> SubItems { get; }
	}
}
