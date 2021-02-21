using Converter.Managers;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Converter.Commands.SaveData
{
    public class SaveDataCommandHandler : IRequestHandler<SaveDataCommand, Unit>
    {
        const string _dataFileName = "DANE";

        public Task<Unit> Handle(SaveDataCommand request, CancellationToken cancellationToken)
        {
            StringBuilder builder = GetDataToSave(request);
            string converterPath = FileManager.GetConverterDirectoryPath(request.ResonatorType, request.ResonatorName);
            
            File.WriteAllText($"{converterPath}\\{_dataFileName}", builder.ToString());

            return Unit.Task;
        }

        StringBuilder GetDataToSave(SaveDataCommand request)
        {
            var builder = new StringBuilder();
            builder.Append($"{request.UnloadedCenterFrequency} {request.UnloadedQ}\n");
            builder.Append($"{request.CenterFrequency}\t{request.QFactor}\t{request.H}\n");

            return builder;
        }

    }
}
