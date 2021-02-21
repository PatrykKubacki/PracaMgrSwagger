using Converter.Factories;
using Converter.Queries.GetResultFromFile.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Converter.Queries.GetResultFromFile
{
    public class GetResultFromFileQueryHandler : IRequestHandler<GetResultFromFileQuery, IResultFromFile>
    {
        const string _resultFileName = "WYNIKI";

        public Task<IResultFromFile> Handle(GetResultFromFileQuery request, CancellationToken cancellationToken)
        {
            if (!FileWithResultExist(request.ResonatorType, request.ResonatorName))
                return null;

            var resultFilePath = $"{GetConverterDictoinaryPath(request.ResonatorType, request.ResonatorName)}\\{_resultFileName}";
            string[] resultFromFile = File.ReadAllLines(resultFilePath);

            var factory = new ResultFromFileFactory();
            var result = factory.CreateResultFromFile(request.ResonatorType, resultFromFile);

            return Task.FromResult(result);
        }

        string GetConverterDictoinaryPath(string resonatorType, string resonatorName)
        {
            string path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\"));
            path = Path.Combine(path, $"Converter\\Converters\\{resonatorType}\\{resonatorName}");

            return path;
        }

        bool FileWithResultExist(string resonatorType, string resonatorName)
        {
            string path = $"{GetConverterDictoinaryPath(resonatorType, resonatorName)}\\{_resultFileName}";

            return File.Exists(path);
        }
    }
}
