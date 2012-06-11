using System;
using FxBackupLib;
using System.IO;

namespace FxBackupTest
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Run ();
		}

		static void Run ()
		{
			string dest = @"c:\temp\fxbtest";
			Directory.Delete (dest, true);
			ItemStore itemStore = new ItemStore (new FileDirectoryMultiStream (dest));
			BackupEngine engine = new BackupEngine (itemStore);
			engine.Origins.Add (new FileSystemOrigin (@"C:\Data\Portable Program Files"));
			engine.Run ();
		}
	}
}
