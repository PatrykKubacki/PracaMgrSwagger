using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Converter.Commands.SaveResult
{
    public class SaveResultCommandHandler : IRequestHandler<SaveResultCommand, Unit>
    {
        const string _dataFileName = "DANE";

        public Task<Unit> Handle(SaveResultCommand request, CancellationToken cancellationToken)
        {
            if (!FileWithDataExist(request.ResonatorType, request.ResonatorName))
                return Unit.Task;

            string converterExePath = GetConverterExePath(request.ResonatorType, request.ResonatorName);
            if(string.IsNullOrEmpty(converterExePath))
                return Unit.Task;

            RunConverterProgram(converterExePath);

            return Unit.Task;
        }

        string GetConverterDictoinaryPath(string resonatorType, string resonatorName)
        {
            string path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\"));
            path = Path.Combine(path, $"Converter\\Converters\\{resonatorType}\\{resonatorName}");

            return path;
        }

        bool FileWithDataExist(string resonatorType, string resonatorName)
        {
            string path = $"{GetConverterDictoinaryPath(resonatorType, resonatorName)}\\{_dataFileName}";

            return File.Exists(path);
        }

        string GetConverterExePath(string resonatorType, string resonatorName)
        {
            string path = GetConverterDictoinaryPath(resonatorType, resonatorName);
            var extensions = new List<string> { "exe" };
            var exeFile = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(s => extensions.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant())).FirstOrDefault();

            return exeFile != null ? Path.GetFullPath(exeFile) : "";
        }

        void RunConverterProgram(string path)
        {
            if (!File.Exists(path))
                return;

            using var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.WorkingDirectory = Path.GetFullPath(Path.Combine(path, @"..\"));
            process.StartInfo.FileName = path;
            process.Start();
        }

    }
}
