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
			Verify (true);
			Verify (false);
		}
		
		static void Backup ()
		{
			Archive archive = new Archive (new DirectoryStore (dest));
			BackupEngine engine = new BackupEngine (archive);
			engine.Origins.Add (new FileSystemOrigin (@"C:\Data\Portable Program Files"));
			engine.Run ();			
		}

		static void Verify (bool hashOnly)
		{
			Archive archive = new Archive (new DirectoryStore (dest));
			VerifyEngine engine = new VerifyEngine (archive);
			engine.Origins.Add (new FileSystemOrigin (@"C:\Data\Portable Program Files"));
			bool same = engine.Run (hashOnly);
			if (same)
				Console.WriteLine ("Verification succeeded");
			else
				Console.WriteLine ("Verification failed");
		}
	}
}
