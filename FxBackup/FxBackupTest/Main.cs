using System;
using FxBackupLib;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace FxBackupTest
{
	class MainClass
	{
		static string originPath = @"C:\Data\Portable Program Files";
		static string archivePath = @"c:\temp\fxbtest";
		static string restorePath = @"c:\temp\fxbtestrest";

		public static void Main (string[] args)
		{
			Run ();
		}

		static void Run ()
		{
			Directory.Delete (archivePath, true);
			Directory.Delete (restorePath, true);
			Directory.CreateDirectory (restorePath);
			Backup ();
			Verify (VerifyEngine.VerificationType.ArchiveHashWithArchiveData);
			Verify (VerifyEngine.VerificationType.ArchiveHashWithOriginData);
			Verify (VerifyEngine.VerificationType.ArchiveDataWithOriginData);
			Restore ();
			TestRestore ();
		}

		static MultiStream GetStore ()
		{
			//return new DirectoryStore (archivePath);
			return new SequenceStream (new DirectoryStore (archivePath));
		}

		static void Backup ()
		{
			using (Archive archive = new Archive (GetStore())) {				
				BackupEngine engine = new BackupEngine (archive);
				engine.Origins.Add (new FileSystemOrigin (originPath));
				engine.Progress += delegate(object sender, OriginProgressEventArgs arg) {
					switch (arg.State) {
					case State.BeginItem:
						Console.WriteLine ("{0}...", arg.OriginItem.Path);
						break;
					case State.Block:
						if (arg.Total > 0)
							Console.WriteLine (
							"       {0} {1}%",
							arg.Done,
							arg.Done * 100 / arg.Total
							);
						else
							Console.WriteLine ("       {0}", arg.Done);
						Console.CursorTop -= 1;
						break;
					}
				};
				engine.Run ();
			}
		}

		static void Verify (VerifyEngine.VerificationType verificationType)
		{
			using (Archive archive = new Archive (GetStore())) {				
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
		
		static void Restore ()
		{
			using (Archive archive = new Archive (GetStore())) {				
				RestoreEngine engine = new RestoreEngine (
				new FileSystemOrigin (restorePath),
				archive
				);
				engine.Progress += delegate(object sender, OriginProgressEventArgs arg) {
					switch (arg.State) {
					case State.BeginItem:
						Console.WriteLine ("{0}...", arg.OriginItem.Path);
						break;
					case State.Block:
						if (arg.Total > 0)
							Console.WriteLine (
							"       {0} {1}%",
							arg.Done,
							arg.Done * 100 / arg.Total
							);
						else
							Console.WriteLine ("       {0}", arg.Done);
						Console.CursorTop -= 1;
						break;
					}
				};
				engine.Run ();
			}
		}

		static void TestRestore ()
		{
			if (TestRecursive (
				originPath,
				Path.Combine(restorePath, Path.GetFileName (originPath))
			))
				Console.WriteLine ("Compare restore succeeded");
			else
				Console.WriteLine ("Compare restore failed");
		}

		static bool TestRecursive (string originPath, string restorePath)
		{
//			Console.WriteLine ("O {0}", originPath);
//			Console.WriteLine ("R {0}", restorePath);
			
			bool same = true;
			
			IEnumerator<FileSystemInfo> origins = (new DirectoryInfo (originPath)).EnumerateFileSystemInfos ()
				.OrderBy (p => p.Name)
				.GetEnumerator ();
			IEnumerator<FileSystemInfo> restores = (new DirectoryInfo (restorePath)).EnumerateFileSystemInfos ()
				.OrderBy (p => p.Name)
				.GetEnumerator ();
			
			FileSystemInfo origin, restore;
			origin = origins.MoveNext () ? origins.Current : null;
			restore = restores.MoveNext () ? restores.Current : null;
			
			while (origin != null || restore != null) {
				int iCompare;
				if (origin != null && restore != null)
					iCompare = string.Compare (
						origin.Name,
						restore.Name,
						StringComparison.InvariantCultureIgnoreCase);
				else if (origin != null)
					iCompare = -1;
				else // if (restore != null)
					iCompare = 1;
								
				if (iCompare < 0) {
					same = false;
					Console.WriteLine ("Item not in restore: {0}", origin.Name);
					origin = origins.MoveNext () ? origins.Current : null;
				} else if (iCompare > 0) {
					same = false;
					Console.WriteLine ("Item not in origin: {0}", restore.Name);
					restore = restores.MoveNext () ? restores.Current : null;
				} else if (iCompare == 0) {
					if ((origin.Attributes & FileAttributes.Directory) != (restore.Attributes & FileAttributes.Directory)) {
						Console.WriteLine ("Mismatch file/directory {0}", origin.FullName);
					} else if ((origin.Attributes & FileAttributes.Directory) != 0) {
						if (!TestRecursive (origin.FullName, restore.FullName))
							same = false;
					} else {
						using (Stream input1 = new FileStream(origin.FullName, FileMode.Open, FileAccess.Read)) {
							using (Stream input2 = new FileStream(restore.FullName, FileMode.Open, FileAccess.Read)) {
								StreamVerifier streamVerifier = new StreamVerifier ();
								if (!streamVerifier.Verify (input1, input2))
									same = false;
							}
						}
					}
					
					origin = origins.MoveNext () ? origins.Current : null;
					restore = restores.MoveNext () ? restores.Current : null;
				}
			}
			
			return same;
		}
	}
}

