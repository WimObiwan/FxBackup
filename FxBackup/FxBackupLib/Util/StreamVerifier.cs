using System;
using System.IO;


namespace FxBackupLib
{
	public class StreamVerifier
	{
		const int BufferSize = 64 * 1024; // 64KB

		byte[] buffer1;
		byte[] buffer2;
		
		public StreamVerifier ()
		{
			buffer1 = new byte[BufferSize];
			buffer2 = new byte[BufferSize];
		}
		
		public bool Verify (Stream input1, Stream input2)
		{
			bool same = true;
			
			int len1, len2;
			do {
				len1 = input1.Read(buffer1, 0, BufferSize);
				len2 = input2.Read(buffer2, 0, BufferSize);
				if (len1 == 0 || len2 == 0) break;
				for (int i = 0; i < len1; i++) {
					if (buffer1 [i] != buffer2 [i]) {
						same = false;
						Console.WriteLine ("Different data");
						break;
					}
				}
			} while (same);
			
			if (len1 > 0 || len2 > 0) {
				same = false;
				Console.WriteLine ("Different lengths");
			}
			
			return same;
		}
	}
}

