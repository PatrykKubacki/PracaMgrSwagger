using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PracaMgrSwagger.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Commands
{
    public class GetSavedMeasurementsSessionCommandHandler : IRequestHandler<GetSavedMeasurementsSessionCommand, IEnumerable<MeasureResult>>
    {
        public Task<IEnumerable<MeasureResult>> Handle(GetSavedMeasurementsSessionCommand request, CancellationToken cancellationToken)
        {
            IEnumerable<MeasureResult> result = new List<MeasureResult>();
            string path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
            path = Path.Combine(path, $"Results\\{request.SessionName}");

            if(File.Exists(path))
                result = JsonConvert.DeserializeObject<IEnumerable<MeasureResult>>(File.ReadAllText(path));

            return Task.FromResult(result);
        }
    }
}
