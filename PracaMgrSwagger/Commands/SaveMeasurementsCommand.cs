using MediatR;
using PracaMgrSwagger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Commands
{
    public class SaveMeasurementsCommand: IRequest<Unit>
    {
        public string SessionName { get; set; }
        public MeasureResult[] Measurements { get; set; }
    }
}
