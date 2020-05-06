using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class MeasureResult
    {
        public double QFactor { get; set; }
        public double CenterFrequencyDifference { get; set; }
        public double CenterFrequency { get; set; }
        public double Bandwidth3dB { get; set; }
        public double PeakTransmittance { get; set; }
        public double SizeH { get; set; }
        public double Permittivity { get; set; }
        public double DielLossTangent { get; set; }
        public double Resistivity { get; set; }
        public double SheetResistance { get; set; }
        public int NumberOrPoints { get; set; }
    }
}
