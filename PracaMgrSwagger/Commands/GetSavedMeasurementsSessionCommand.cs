using MediatR;
using PracaMgrSwagger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Commands
{
    public class GetSavedMeasurementsSessionCommand : IRequest<IEnumerable<MeasureResult>>
    {
        public string SessionName { get; set; }
    }
}
