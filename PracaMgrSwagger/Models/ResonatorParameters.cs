using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class ResonatorParameters
    {
        public double QFactor { get; set; }
        public double CenterFrequency { get; set; }
        public double PeakTransmittance { get; set; }

        public ResonatorParameters()
        {

        }

        public ResonatorParameters(double qFactor, double centerFrequency, double peakTransmittance)
        {
            QFactor = qFactor;
            CenterFrequency = centerFrequency;
            PeakTransmittance = peakTransmittance;
        }
    }
}
