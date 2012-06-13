using System;
using System.IO;
using System.Security.Cryptography;


namespace FxBackupLib
{
	public class StreamPump
	{
		const int BufferSize = 64 * 1024; // 64KB

		byte[] buffer;
		HashAlgorithm hashAlgorithm = new SHA512Managed();
		
		public delegate void ProgressCallback(long done, long total);
		public ProgressCallback Progress;

		public StreamPump ()
		{
			buffer = new byte[BufferSize];
		}
		
		public void Copy (Stream input, Stream output, out byte[] hash)
		{
			hashAlgorithm.Initialize ();
			
			long done = 0;
			long total = input.CanSeek ? input.Length : -1;
			
			int len;
			while ((len = input.Read(buffer, 0, BufferSize)) > 0) {
				hashAlgorithm.TransformBlock (buffer, 0, len, buffer, 0);
				output.Write (buffer, 0, len);
				done += len;
				
				if (Progress != null) {
					Progress(done, total);
				}
			}
			hashAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
			hash = hashAlgorithm.Hash;
		}
	}
}

