using System;
using System.Collections.Generic;

namespace FxBackupLib
{
	public interface IOriginProvider : IDisposable
	{
		IOriginProviderItem GetNextItem();
	}
}

