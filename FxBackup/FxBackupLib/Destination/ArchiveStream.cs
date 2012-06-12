using System;
namespace FxBackupLib
{
	public class ArchiveStream
	{
		public Guid StreamId { get; private set; }
		public Guid PhysicalStoreStreamId { get; set; }
		public byte[] Hash { get; set; }
		
		public ArchiveStream (Guid streamId)
		{
			StreamId = streamId;
		}
	}
}

