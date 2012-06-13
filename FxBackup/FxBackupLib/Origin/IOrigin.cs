using System;
using System.Collections.Generic;
using System.IO;


namespace FxBackupLib
{
	public interface IOrigin
	{
		IOriginItem GetRootItem();
	}
}

