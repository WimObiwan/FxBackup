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
			Verify (VerifyEngine.VerificationType.ArchiveHashWithArchiveData);
			Verify (VerifyEngine.VerificationType.ArchiveHashWithOriginData);
			Verify (VerifyEngine.VerificationType.ArchiveDataWithOriginData);
			//Restore ();
		}
		
		static void Backup ()
		{
			Archive archive = new Archive (new DirectoryStore (dest));
			BackupEngine engine = new BackupEngine (archive);
			engine.Origins.Add (new FileSystemOrigin (@"C:\Data\Portable Program Files"));
			engine.Progress += delegate(object sender, ProgressEventArgs arg) {
				switch (arg.State) {
				case State.BeginItem:
					Console.WriteLine ("{0}...", arg.OriginItem.Path);
					break;
				case State.BeginStream:
					Console.WriteLine ("   * Stream {0}", arg.OriginItemStream.Name);
					break;
				case State.Block:
					if (arg.Total > 0)
						Console.WriteLine ("       {0} {1}%", arg.Done, arg.Done * 100 / arg.Total);
					else
						Console.WriteLine ("       {0}", arg.Done);
					Console.CursorTop -= 1;
					break;
				}
			};
			engine.Run ();			
		}

		static void Verify (VerifyEngine.VerificationType verificationType)
		{
			Archive archive = new Archive (new DirectoryStore (dest));
			VerifyEngine engine = new VerifyEngine (archive);
			if (verificationType != VerifyEngine.VerificationType.ArchiveHashWithArchiveData)
				engine.Origins.Add (new FileSystemOrigin (@"C:\Data\Portable Program Files"));
			bool same = engine.Run (verificationType);
			if (same)
				Console.WriteLine ("Verification succeeded");
			else
				Console.WriteLine ("Verification failed");
		}
	}
}
