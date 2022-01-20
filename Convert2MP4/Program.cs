using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;

namespace Convert2MP4
{
	class Settings
	{
		public string Pattern { get; set; }

		public string Cmd { get; set; }

		public string Arg { get; set; }

		public bool KeepLookup { get; set; }

		public bool CleanupMode { get; set; }

		public int CleanupDays { get; set; }
	}

	class Program
	{
		static async Task Main(string[] args)
		{
			Settings settings = new Settings()
			{
				Pattern = ".*\\.(flv)$",
				KeepLookup = true,
				CleanupMode = false,
				CleanupDays = 3
			};
			Regex regex = new Regex(settings.Pattern);
			List<String> list = new List<string>();
			if (args.Length <= 0)
			{
				Console.WriteLine(" * No args, please select file here. One file each line, leave blank and enter to complete.");
				Console.WriteLine(" * -cm		Cleanup Mode.");
				String s;
				do
				{
					s = Console.ReadLine();
					list.Add(s);
				}
				while (!String.IsNullOrEmpty(s));
			}
			else
			{
				foreach (string s in args)
				{
					list.Add(s);
				}
			}
			if (String.IsNullOrEmpty(list[^1]))
			{
				list.RemoveAt(list.Count - 1);
			}
			if (list.Count <= 0)
			{
				Console.WriteLine(" * File list does not contains any file.");
				Environment.Exit(0);
			}
			List<String> queue = new List<string>();
			foreach (string s in list)
			{
				if (s.Trim().StartsWith("-cm"))
				{
					settings.CleanupMode = true;
					try
					{
						int? days = int.Parse(s.Substring(3, s.Length - 3));
						settings.CleanupDays = days ?? 3;
					}
					catch (Exception e)
					{
						Console.WriteLine(" * Parse Error, use default cleanup settings: " + settings.CleanupDays);
					}
					continue;
				}
				LookupFilesInDirectory(s, ref queue, regex, settings.KeepLookup);
			}
			Console.WriteLine("**********  Task Info  **********");
			foreach (String s in queue)
			{
				Console.WriteLine(" * Source: " + s);
			}
			Console.WriteLine("**********  Task Info  **********");
			Console.WriteLine();
			ProcessStartInfo ffmpeg4Info = new ProcessStartInfo() { CreateNoWindow = false, FileName = "ffmpeg.exe" };
			int c = 1, t = queue.Count;
			List<Task> tasks = new List<Task>();
			foreach (string s in queue)
			{
				Console.WriteLine(" * Task ( " + c + " / " + t + " ): " + s);
				// Console.WriteLine(Path.GetDirectoryName(s));
				// Console.WriteLine(Path.GetFileName(s));
				// Console.WriteLine(Path.GetFileNameWithoutExtension(s));
				// Console.WriteLine(Path.GetExtension(s));
				if (File.Exists(s) && !settings.CleanupMode)
				{
					// String arg = "-i \"" + s + "\" -c copy -copyts \"" + Path.GetFullPath(s).Replace(Path.GetExtension(s), ".mp4\"");
					String arg = "-y -i \"" + s + "\" -c copy -copyts \"" + Path.GetDirectoryName(s) + "\\" + Path.GetFileNameWithoutExtension(s) + ".mp4\"";
					ffmpeg4Info.Arguments = arg;
					Process ffmpeg = new Process() { StartInfo = ffmpeg4Info };
					// ffmpeg.OutputDataReceived += (sender, e) =>
					// {
					//     if (!String.IsNullOrEmpty(e.Data))
					//     {
					//         Console.WriteLine(e.Data);
					//     }
					// };
					// ffmpeg.ErrorDataReceived += (sender, e) =>
					// {
					//     if (!String.IsNullOrEmpty(e.Data))
					//     {
					//         Console.WriteLine(e.Data);
					//     }
					// };
					Console.WriteLine(" * " + ffmpeg4Info.FileName + " " + arg);
					// String env = Environment.GetEnvironmentVariable("PATH");
					// foreach (var str in String.IsNullOrEmpty(env) ? String.Empty : env)
					// {
					//     Console.WriteLine(str);
					// }
#if !DEBUG
					try
					{
						do
						{
							ffmpeg.Start();
							// ffmpeg.BeginOutputReadLine();
							ffmpeg.WaitForExit();
							Console.WriteLine(" * ffmpeg exit with: " + ffmpeg.ExitCode);
						}
						while (ffmpeg.ExitCode != 0);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
						throw;
					}
#endif
				}
				else if (File.Exists(s) && settings.CleanupMode)
				{
					string[] str = Path.GetFileNameWithoutExtension(s).Split(char.Parse("-"));
					DateTime time = DateTime.Parse(str[2].Insert(6, "-").Insert(4, "-"));
					if (time.AddDays(double.Parse(settings.CleanupDays.ToString())) <= DateTime.Now && str[1]!="19625")
					{
						var task = DeleteRawFileAsync(s);
						tasks.Add(task);
					}
				}
				else
				{
					Console.WriteLine(" * File not exist or already .mp4 file.");
				}
				Console.WriteLine(" * Task ( " + c++ + " / " + t + " ) Completed!");
				Console.WriteLine();
			}
			if (!settings.CleanupMode)
			{
				Console.WriteLine(" * Delete raw file? (Y/N)");
				String select;
				do
				{
					select = Console.ReadLine();
				}
				while (String.IsNullOrEmpty(select));
				if (select.ToLower() == "y" || select.ToLower() == "yes")
				{
					foreach (String s in queue)
					{
						// Console.WriteLine(" * Deleting: " + Path.GetFileName(s1));
						// File.Delete(s1);
						// Console.WriteLine(" * Deleted: " + Path.GetFileName(s1));

						var task = DeleteRawFileAsync(s);
						tasks.Add(task);
					}
				}
			}
			await Task.WhenAll(tasks);
			Console.WriteLine();
			Console.WriteLine(" * Total delete tasks: "+tasks.Count);
			Console.WriteLine();
			Thread.Sleep(2 * 1000);
		}

		private static void LookupFilesInDirectory(string s, ref List<string> fileQueue, Regex regex, bool ifKeepLookup)
		{
			if (!File.Exists(s) && Directory.Exists(s)) // 传进来的 s 是目录的情况下递归添加文件
			{
				Console.WriteLine(" * Directory detected. Loading files...");
				String[] filesInListDir = Directory.GetFileSystemEntries(s); // 读目录下的文件与文件夹
				foreach (string file in filesInListDir)
				{
					if (Directory.Exists(file) && ifKeepLookup) // 目录下还有目录且设置递归查找为真
					{
						LookupFilesInDirectory(file, ref fileQueue, regex, true);
					}
					if (regex.IsMatch(file))
					{
						Console.WriteLine(" * Match! Adding file: " + file); // 添加目录下符合条件的文件
						fileQueue.Add(file);
					}
				}
			}
			else if (File.Exists(s)) // 传进来的 s 本身就是文件的情况下
			{
				if (regex.IsMatch(s))
				{
					Console.WriteLine(" * Match! Adding file: " + s);
					fileQueue.Add(s);
				}
			}
		}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		static async Task DeleteRawFileAsync(String file)
		{
			Console.WriteLine(" * Deleting: " + Path.GetFileName(file));
#if !DEBUG
			await Task.Run(() => { File.Delete(file); });
			Console.WriteLine(" * Deleted: " + Path.GetFileName(file));
#endif
		}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	}
}