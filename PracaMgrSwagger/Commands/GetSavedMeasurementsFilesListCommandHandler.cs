using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Commands
{
    public class GetSavedMeasurementsFilesListCommandHandler : IRequestHandler<GetSavedMeasurementsFilesListCommand, List<string>>
    {
        public Task<List<string>> Handle(GetSavedMeasurementsFilesListCommand request, CancellationToken cancellationToken)
        {
            string path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
            path = Path.Combine(path, $"Results\\");
            var results = new List<string>();

            if (Directory.Exists(path))
            {
                var filesPaths = Directory.GetFiles(path).ToList();
                foreach (var filePath in filesPaths)
                {
                    FileInfo fileInfo = new (filePath);
                    results.Add(fileInfo.Name);
                }
            }

            return Task.FromResult(results);
        }
    }
}
