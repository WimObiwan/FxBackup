using System;
using FxBackupLib;

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
			BackupEngine engine = new BackupEngine ();
			engine.Origins.Add (new FileSystemOrigin (@"C:\Data\Portable Program Files"));
			engine.Run ();
		}
	}
}
