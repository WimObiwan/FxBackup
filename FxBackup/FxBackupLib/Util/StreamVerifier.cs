using System;
using System.IO;
using System.Security.Cryptography;

namespace FxBackupLib
{
	public class StreamVerifier
	{
		const int BufferSize = 64 * 1024; // 64KB

		byte[] buffer1;
		byte[] buffer2;
		HashAlgorithm hashAlgorithm = new SHA512Managed ();
		
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
				len1 = input1.Read (buffer1, 0, BufferSize);
				len2 = input2.Read (buffer2, 0, BufferSize);
				if (len1 == 0 || len2 == 0)
					break;
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
		
		public bool Verify (Stream input, byte[] hash)
		{
			bool same = true;
			hashAlgorithm.Initialize ();
			
			int len;
			while ((len = input.Read (buffer1, 0, BufferSize)) > 0) {			       
				hashAlgorithm.TransformBlock (buffer1, 0, len, buffer1, 0);
			}
			
			hashAlgorithm.TransformFinalBlock (new byte[0], 0, 0);
			
			if (hash.Length != hashAlgorithm.Hash.Length) {
			} else {
				for (int i = 0; i < hash.Length; i++) {
					if (hash [i] != hashAlgorithm.Hash [i]) {
						same = false;
						Console.WriteLine ("Different hash");
						break;
					}
				}
			}
			hashAlgorithm.Clear ();
			
			return same;
		}
	}
}

