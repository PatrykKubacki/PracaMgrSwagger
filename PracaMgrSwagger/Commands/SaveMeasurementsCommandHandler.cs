using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Commands
{
    public class SaveMeasurementsCommandHandler : IRequestHandler<SaveMeasurementsCommand, Unit>
    {
        public Task<Unit> Handle(SaveMeasurementsCommand request, CancellationToken cancellationToken)
        {
            var serializedMeasurements = JsonConvert.SerializeObject(request.Measurements);
            string path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
            path = Path.Combine(path, $"Results\\{request.SessionName}");

            if(File.Exists(path))
                File.WriteAllText(path, serializedMeasurements);
            else
                File.WriteAllText(path, serializedMeasurements);

            return Task.FromResult(Unit.Value);
        }
    }
}
