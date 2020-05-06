using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models.Settings
{
    public class MainSettings
    {
        public ResonatorType DefaultResonatorType { get; set; }
        public string SerialPort { get; set; }
        public int Oversampling { get; set; }
        public int Averaging { get; set; }
        public MeasType MeasType { get; set; }
        public Algorithm Algorithm { get; set; }
        public bool UnloadedQCorrection { get; set; }
    }

    public enum MeasType
    {
        S21 = 1,
        Power = 2,
    }


    public enum Algorithm
    {
        Bandwidth3dB = 1,
        LeastSquareFit = 2,
    }

}
