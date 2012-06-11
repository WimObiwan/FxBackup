using System;
namespace FxBackupLib
{
	public interface IOriginProviderItem
	{
		OriginProviderItemStream GetNextStream();
	}
}

