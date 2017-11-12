#define USEWHITELIST
//#define USEBLACKLIST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace FileMove
{
    class Program
    {
        static readonly string[] bannedstr = { ".txt", ".url", ".torrent", ".zip", " .7z", ".bat", ".db", ".rar", ".doc" };
        static readonly string[] whitelist = { ".png", ".jpg", ".jpeg", ".gif" };
        static HashSet<string> BannedExtensions = new HashSet<string>();
        static HashSet<string> AcceptedExtensions = new HashSet<string>();
        static HashSet<string> Extensions = new HashSet<string>();

        static void Main(string[] args)
        {
#if USEBLACKLIST
            foreach (string str in bannedstr)
                BannedExtensions.Add(str);
#elif USEWHITELIST
            foreach (string str in whitelist)
                AcceptedExtensions.Add(str);
#endif
            string CurrentDir = System.Environment.CurrentDirectory;
            if (!WelcomeProcedure())
                return;
            seekChildPathRecu(CurrentDir, CurrentDir);
            Console.WriteLine();
            Console.WriteLine("Extensions:");
            foreach (string extension in Extensions)
                Console.Write(extension + " ");
            Console.ReadKey();
        }

        static void seekChildPathRecu(string CurrentPath, string TargetPath)
        {
            // delete unexpected Extension
            if(CurrentPath != TargetPath)
                DeleteUnexpectedExtension(CurrentPath);
            string[] pathArr = Directory.GetDirectories(CurrentPath);
            var tmpstr = CurrentPath.Split('\\');
            string filePath = tmpstr[tmpstr.Length - 1];
            // has subfolder(s)
            if (pathArr.Length != 0)
            {
                foreach (string path in pathArr)
                    seekChildPathRecu(path, TargetPath);
                if (CurrentPath == TargetPath)
                    return;
                // delete folder which only contains subfolder(s)
                try
                {
                    if (Directory.Exists(CurrentPath) && Directory.GetFiles(CurrentPath).Length == 0)
                    {
                        Directory.Delete(CurrentPath);
                        ConsoleWriteProcess("Deleted Folder: ", CurrentPath);
                        return;
                    }
                }
                catch (System.IO.IOException)
                {
                    ConsoleWriteWarning("Warning: \"" + CurrentPath + "\" Is Not Empty");
                }
                catch (Exception e)
                {
                    ConsoleWriteWarning(e.Message);
                }
                // Don't Have To Move File
                if (CurrentPath == TargetPath + '\\' + filePath)
                    return;
                var files = Directory.GetFiles(CurrentPath);
                if (files.Length != 0)
                {
                    MoveFiles(CurrentPath, TargetPath);
                }
            }
            else
            {
                // delete this folder if it's empty
                if (Directory.GetFiles(CurrentPath).Length == 0)
                {
                    Directory.Delete(CurrentPath);
                    ConsoleWriteProcess("Deleted Folder: ", CurrentPath);
                    return;
                }
                // return if root path
                if (CurrentPath == TargetPath + '\\' + filePath)
                    return;
                try
                {
                    Directory.Move(CurrentPath, TargetPath + '\\' + filePath);
                }
                catch (System.IO.IOException)
                {
                    // Move Files Instead of Directory
                    MoveFiles(CurrentPath, TargetPath);
                }
            }
        }

        static void MoveFiles(string CurrentPath, string TargetPath)
        {
            var tmpstr = CurrentPath.Split('\\');
            string filePath = tmpstr[tmpstr.Length - 1];
            if (!Directory.Exists(TargetPath + '\\' + filePath))
            {
                Directory.CreateDirectory(TargetPath + '\\' + filePath);
            }
            foreach (string file in Directory.GetFiles(CurrentPath))
            {
                var tmpfilename = file.Split('\\');
                string fileName = tmpfilename[tmpfilename.Length - 1];
                try
                {
                    File.Move(file, TargetPath + '\\' + filePath + '\\' + fileName);
                }
                catch (Exception e)
                {
                    ConsoleWriteWarning(e.Message);
                }
            }
            // delete folder which contains file(s)
            try
            {
                Directory.Delete(CurrentPath);
                ConsoleWriteProcess("Deleted Folder: ", CurrentPath);
            }
            catch (System.IO.IOException)
            {
                ConsoleWriteWarning("Warning: \"" + CurrentPath + "\" Is Not Empty");
            }
        }

        static void ConsoleWriteWarning(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(s);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void ConsoleWriteProcess(string ProcessName, string Content)
        {
            ConsoleWriteProcess(ProcessName, Content, ConsoleColor.DarkGreen);
        }

        static void ConsoleWriteProcess(string ProcessName, string Content, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(ProcessName);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(Content);
        }
        static void DeleteUnexpectedExtension(string path)
        {
            // delete unexpected Extension
            foreach (var file in Directory.GetFiles(path))
            {
                string fileExtension = Path.GetExtension(file).ToLower();
                Extensions.Add(fileExtension);
#if USEBLACKLIST
                if (BannedExtensions.Contains(fileExtension))
                {
                    File.Delete(file);
                    ConsoleWriteProcess("Deleted File: ", file, ConsoleColor.Green);
                }
#elif USEWHITELIST
                if (!AcceptedExtensions.Contains(fileExtension))
                {
                    File.Delete(file);
                    ConsoleWriteProcess("Deleted File: ", file, ConsoleColor.Green);
                }
#endif
            }
        }

        static bool WelcomeProcedure()
        {
            ConsoleWriteProcess("You Are Trying To Pull All Folders Above In:\n", System.Environment.CurrentDirectory, ConsoleColor.Cyan);
            ConsoleWriteWarning("Are You Serious?(Y/N)");
            if (Console.ReadKey().Key == ConsoleKey.Y)
            {
                Console.Write('\b');
                ConsoleWriteProcess("Starting", "");
                return true;
            }
            Console.Write('\b');
            ConsoleWriteProcess("Shutting Down","");
            Thread.Sleep(1000);
            return false;
        }
    }
}
