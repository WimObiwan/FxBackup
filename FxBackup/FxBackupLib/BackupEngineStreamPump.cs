using System;
using System.IO;


namespace FxBackupLib
{
	public class BackupEngineStreamPump
	{
		const int BufferSize = 64 * 1024; // 64KB

		byte[] buffer;
		
		public BackupEngineStreamPump ()
		{
			buffer = new byte[BufferSize];
		}
		
		public void Pump(Stream input, Stream output)
		{
			int len;
			while ((len = input.Read(buffer, 0, BufferSize)) > 0) {
				//output.Write(buffer, 0, len);
			}
		}
	}
}

