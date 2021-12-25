using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Convert2MP4
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<String> list = new List<string>();
            if (args.Length <= 0)
            {
                Console.WriteLine(" * No args, please select file here. One file each line, leave blank and enter to complete.");
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
            Console.WriteLine("**********  Task Info  **********");
            foreach (String s in list)
            {
                Console.WriteLine(" * Source: " + s);
            }
            Console.WriteLine("**********  Task Info  **********");
            Console.WriteLine();
            // ProcessStartInfo ffmpeg4Info = new ProcessStartInfo() {CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true,RedirectStandardInput = true, FileName = "ffmpeg.exe"};
            ProcessStartInfo ffmpeg4Info = new ProcessStartInfo() { CreateNoWindow = false, FileName = "ffmpeg.exe" };
            int c = 1, t = list.Count;
            foreach (string s in list)
            {
                if (String.IsNullOrEmpty(s))
                {
                    break;
                }
                Console.WriteLine(" * Task ( " + c + " / " + t + " ): " + s);
                // Console.WriteLine(Path.GetDirectoryName(s));
                // Console.WriteLine(Path.GetFileName(s));
                // Console.WriteLine(Path.GetFileNameWithoutExtension(s));
                // Console.WriteLine(Path.GetExtension(s));
                if (File.Exists(s) && Path.GetExtension(s) != ".mp4")
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
                }
                else
                {
                    Console.WriteLine(" * File not exist or already .mp4 file.");
                }
                Console.WriteLine(" * Task ( " + c++ + " / " + t + " ) Completed!");
                Console.WriteLine();
            }
            Console.WriteLine(" * Delete raw file? (Y/N)");
            String select;
            do
            {
                select = Console.ReadLine();
            }
            while (String.IsNullOrEmpty(select));
            if (select.ToLower() == "y" || select.ToLower() == "yes")
            {
                List<Task> tasks = new List<Task>();
                foreach (String s in list)
                {
                    // Console.WriteLine(" * Deleting: " + Path.GetFileName(s1));
                    // File.Delete(s1);
                    // Console.WriteLine(" * Deleted: " + Path.GetFileName(s1));

                    var task = DeleteRawFileAsync(s);
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
            }
            Console.WriteLine();
            // Console.ReadLine();
            Thread.Sleep(2 * 1000);
        }

        static async Task DeleteRawFileAsync(String file)
        {
            Console.WriteLine(" * Deleting: " + Path.GetFileName(file));
            await Task.Run(() => { File.Delete(file); });
            Console.WriteLine(" * Deleted: " + Path.GetFileName(file));
        }
    }
}