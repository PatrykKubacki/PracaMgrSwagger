using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models.Requests
{
    public class SaveMeasurmentsRequest
    {
        public string SessionName { get; set; }
        public MeasureResult[] Measurements { get; set; }
    }
}
