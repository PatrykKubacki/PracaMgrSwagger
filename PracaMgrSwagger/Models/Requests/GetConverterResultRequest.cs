using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models.Requests
{
    public class GetConverterResultRequest
    {
        public string UnloadedCenterFrequency { get; set; }
        public string UnloadedQ { get; set; }
        public string CenterFrequency { get; set; }
        public string H { get; set; }
        public string QFactor { get; set; }
        public string ResonatorType { get; set; }
        public string ResonatorName { get; set; }
    }
}
