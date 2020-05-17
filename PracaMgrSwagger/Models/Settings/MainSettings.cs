using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models.Settings
{
    public class MainSettings
    {
        public string DefaultResonatorType { get; set; }
        public string SerialPort { get; set; }
        public int Oversampling { get; set; }
        public int Averaging { get; set; }
        public string MeasType { get; set; }
        public string Algorithm { get; set; }
        public bool UnloadedQCorrection { get; set; }
        public bool AutomaticMeasurement { get; set; }
    }

}
