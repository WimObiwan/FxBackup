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
		
		public StreamPump ()
		{
			buffer = new byte[BufferSize];
		}
		
		public void Copy (Stream input, Stream output, out byte[] hash)
		{
			hashAlgorithm.Initialize ();
			int len;
			while ((len = input.Read(buffer, 0, BufferSize)) > 0) {
				hashAlgorithm.TransformBlock (buffer, 0, len, buffer, 0);
				output.Write(buffer, 0, len);
			}
			hashAlgorithm.TransformFinalBlock(new byte[0], 0, 0);
			hash = hashAlgorithm.Hash;
		}
	}
}

