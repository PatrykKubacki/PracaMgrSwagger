using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class MeasureResult
    {
        public string SampleName { get; set; }
        public string H { get; set; }
        public string Permittivity { get; set; }
        public string DielLossTangent { get; set; }
        public string Resistivity { get; set; }
        public string SheetResistance { get; set; }
        public double Q { get; set; }
        public double FrequencyDifference { get; set; }
        public double F0 { get; set; }
        public double Bw { get; set; }
        public double Peak { get; set; }
        public int Points { get; set; }
    }
}
