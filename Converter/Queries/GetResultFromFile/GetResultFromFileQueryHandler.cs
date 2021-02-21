using Converter.Factories;
using Converter.Managers;
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
            Thread.Sleep(500);
            if (!FileManager.FileInConverterDirectoryExist(request.ResonatorType, request.ResonatorName, _resultFileName))
                return null;

            var resultFilePath = $"{FileManager.GetConverterDirectoryPath(request.ResonatorType, request.ResonatorName)}\\{_resultFileName}";
            string[] resultFromFile = File.ReadAllLines(resultFilePath);

            var factory = new ResultFromFileFactory();
            var result = factory.CreateResultFromFile(request.ResonatorType, resultFromFile);

            return Task.FromResult(result);
        }

    }
}
