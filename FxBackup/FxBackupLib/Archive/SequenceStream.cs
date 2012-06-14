using System;
using System.Collections.Generic;
using System.IO;

namespace FxBackupLib
{
	public class SequenceStream : MultiStream
	{
		readonly static Guid indexStreamId = new Guid ("338d2fa6-ad9f-4f14-922e-a5baf335c3d8");
		readonly static Guid dataStreamId = new Guid ("345cb29c-3d70-4ef5-b29d-a310e20ace4c");
		MultiStream multiStream;
		Stream dataStream;
		
		class DictionaryItem
		{
			public Guid StreamId { get; set; }

			public long Position { get; set; }

			public long Length { get; set; }
		}
		
		Dictionary<Guid, DictionaryItem> index;
		bool indexChanged;
		
		public SequenceStream (MultiStream multiStream)
		{
			this.multiStream = multiStream;
		}

		class SubStream : Stream
		{
			Stream stream;
			DictionaryItem item;
			long subPosition;
			
			public SubStream (Stream stream, DictionaryItem item)
			{
				this.stream = stream;
				this.item = item;
				stream.Position = item.Position;
			}
				
			#region implemented abstract members of System.IO.Stream
			public override void Flush ()
			{
				stream.Flush ();
			}

			public override int Read (byte[] buffer, int offset, int count)
			{
				count = Math.Min (count, (int)(item.Length - subPosition));
				int len = stream.Read (buffer, offset, count);
				subPosition += len;
				return len;
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				long newPosition;
				switch (origin) {
				case SeekOrigin.Begin:
					newPosition = offset;
					break;
				case SeekOrigin.Current:
					newPosition = subPosition + offset;
					break;
				case SeekOrigin.End:
					newPosition = item.Length + offset;
					break;
				default:
					throw new InvalidOperationException ();
				}
			
				Position = newPosition;
				return newPosition;
			}

			public override void SetLength (long value)
			{
				throw new System.NotSupportedException ();
			}

			public override void Write (byte[] buffer, int offset, int count)
			{
//				if (length > 0)
//					throw new NotSupportedException ();
				
				stream.Write (buffer, offset, count);
				this.subPosition += count;
				if (subPosition > item.Length)
					item.Length = subPosition;
			}

			public override bool CanRead {
				get {
					return stream.CanRead;
				}
			}

			public override bool CanSeek {
				get {
					return stream.CanSeek;
				}
			}

			public override bool CanWrite {
				get {
					return stream.CanWrite;
				}
			}

			public override long Length {
				get {
					return item.Length;
				}
			}

			public override long Position {
				get {
					System.Diagnostics.Debug.Assert (stream.Position - item.Position == subPosition);
					return subPosition;
				}
				set {
					if (value < 0 || value >= item.Length)
						throw new InvalidOperationException ();
					
					stream.Position = item.Position + value;
				}
			}
			#endregion

			void AssertValidPosition (long newPosition)
			{
			}
		}
		
		#region implemented abstract members of FxBackupLib.MultiStream
		protected override Stream CreateStreamImpl (Guid id)
		{
			if (index == null) {
				ReadIndex ();
			}
			
			if (dataStream == null) {
				OpenDataStream ();
			}
			
			DictionaryItem item = new DictionaryItem ();
			item.StreamId = id;
			item.Position = dataStream.Position;
			item.Length = -1;
			index.Add (id, item);
			indexChanged = true;

			return new SubStream (dataStream, item);
		}

		protected override Stream OpenStreamImpl (Guid id)
		{
			if (index == null)
				ReadIndex ();
			
			if (dataStream == null) {
				OpenDataStream ();
			}
			
			DictionaryItem item;
			if (!index.TryGetValue (id, out item))
				throw new InvalidOperationException (string.Format (
					"Stream {0} not found",
					id
				)
				);
			
			return new SubStream (dataStream, item);
		}

		protected override bool ExistsImpl (Guid id)
		{
			if (index == null)
				ReadIndex ();
				
			return index.ContainsKey (id);
		}
		
		protected override void CloseImpl ()
		{
			if (index != null) {
				WriteIndex ();
				index = null;
			}
			
			if (dataStream != null) {
				dataStream.Close ();
				dataStream = null;
			}
		}
		#endregion

		void ReadIndex ()
		{
			index = new Dictionary<Guid, DictionaryItem> ();
			if (multiStream.Exists (indexStreamId)) {
				using (BinaryReader reader = new BinaryReader(multiStream.OpenStream(indexStreamId))) {
					int cnt = reader.ReadInt32 ();
					while (cnt-- > 0) {
						DictionaryItem item = new DictionaryItem ();
						item.StreamId = new Guid (reader.ReadBytes (16));
						item.Position = reader.ReadInt64 ();
						item.Length = reader.ReadInt64 ();
						index.Add (item.StreamId, item);
					}
				}
			}
			indexChanged = false;
		}

		void WriteIndex ()
		{
			if (indexChanged) {
				using (BinaryWriter writer = new BinaryWriter(multiStream.CreateStream(indexStreamId))) {
					writer.Write (index.Count);
					foreach (DictionaryItem item in index.Values) {
						writer.Write (item.StreamId.ToByteArray ());
						writer.Write (item.Position);
						writer.Write (item.Length);
					}
				}
				indexChanged = false;
			}
		}

		void OpenDataStream ()
		{
			if (multiStream.Exists (dataStreamId))
				dataStream = multiStream.OpenStream (dataStreamId);
			else
				dataStream = multiStream.CreateStream (dataStreamId);
		}
	}
}

