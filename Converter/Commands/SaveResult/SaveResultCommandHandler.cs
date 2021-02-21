using Converter.Managers;
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
            if (!FileManager.FileInConverterDirectoryExist(request.ResonatorType, request.ResonatorName, _dataFileName))
                return Unit.Task;

            string converterExePath = FileManager.GetConverterExePath(request.ResonatorType, request.ResonatorName);
            if(string.IsNullOrEmpty(converterExePath))
                return Unit.Task;

            RunConverterProgram(converterExePath);

            return Unit.Task;
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
