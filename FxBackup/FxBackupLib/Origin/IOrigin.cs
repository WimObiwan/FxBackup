using System;
using System.Collections.Generic;


namespace FxBackupLib
{
	public interface IOrigin
	{
		IOriginItem GetRootItem();
	}
}

