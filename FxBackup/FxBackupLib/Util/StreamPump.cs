using System;
using System.IO;


namespace FxBackupLib
{
	public class StreamPump
	{
		const int BufferSize = 64 * 1024; // 64KB

		byte[] buffer;
		
		public StreamPump ()
		{
			buffer = new byte[BufferSize];
		}
		
		public void Copy(Stream input, Stream output)
		{
			int len;
			while ((len = input.Read(buffer, 0, BufferSize)) > 0) {
				output.Write(buffer, 0, len);
			}
		}
	}
}

