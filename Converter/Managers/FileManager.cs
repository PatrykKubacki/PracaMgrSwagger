using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Converter.Managers
{
    public static class FileManager
    {
        public static string GetConverterDirectoryPath(string resonatorType, string resonatorName)
        {
            string path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\"));
            path = Path.Combine(path, $"Converter\\Converters\\{resonatorType}\\{resonatorName}");

            return path;
        }

        public static bool FileInConverterDirectoryExist(string resonatorType, string resonatorName, string fileName)
        {
            string path = $"{FileManager.GetConverterDirectoryPath(resonatorType, resonatorName)}\\{fileName}";
            return File.Exists(path);
        }

        public static string GetConverterExePath(string resonatorType, string resonatorName)
        {
            string path = FileManager.GetConverterDirectoryPath(resonatorType, resonatorName);
            var extensions = new List<string> { "exe" };
            var exeFile = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(s => extensions.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant())).FirstOrDefault();

            return exeFile != null ? Path.GetFullPath(exeFile) : "";
        }

    }
}
