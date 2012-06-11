using System;
using FxBackupLib;
using System.IO;

namespace FxBackupTest
{
	class MainClass
	{
		static string dest = @"c:\temp\fxbtest";
		public static void Main (string[] args)
		{
			Run ();
		}

		static void Run ()
		{
			Directory.Delete (dest, true);
			Backup ();
			Verify ();
		}
		
		static void Backup ()
		{
			ItemStore itemStore = new ItemStore (new FileDirectoryMultiStream (dest));
			BackupEngine engine = new BackupEngine (itemStore);
			engine.Origins.Add (new FileSystemOrigin (@"C:\Data\Portable Program Files"));
			engine.Run ();			
		}

		static void Verify ()
		{
			ItemStore itemStore = new ItemStore (new FileDirectoryMultiStream (dest));
			VerifyEngine engine = new VerifyEngine (itemStore);
			engine.Origins.Add (new FileSystemOrigin (@"C:\Data\Portable Program Files"));
			bool same = engine.Run ();
			if (same)
				Console.WriteLine ("Verification succeeded");
			else
				Console.WriteLine ("Verification failed");
		}
	}
}
