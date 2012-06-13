using System;
using System.Collections.Generic;
using System.IO;


namespace FxBackupLib
{
	public interface IOriginItem
	{
		string Name { get; }
		string Path { get; }
		int Type { get; }
		Stream OpenStream();
		Stream CreateStream();
		IOriginItem CreateChildItem(string name, int type);
		System.Collections.Generic.IEnumerable<IOriginItem> ChildItems { get; }
	}
}
