using System;
using System.Collections.Generic;
using System.Text;

namespace QFactorCalculator
{
    public class QFactorSettings
    {
        public enum Algorithms { Algorithm3dB, Algorithm10dB, AlgorithmIterativeLorenz, AlgorithmLeastSquareLorenz };

        Algorithms _algorithm;       // Algorithm used to calculate the Q-Factor
        double _centerFrequency;     // Center frequency from which we are starting looking for the transmittance peak
        double _spanFrequency;       // Range of frequencies to look for the transmittance peak
        bool _autoFrequency;         // Should be set to TRUE when center- and spanFrequency are not known

        public QFactorSettings(Algorithms algorithm, double centerFrequency, double spanFrequency, bool autoFrequency)
        {
            _algorithm = algorithm;
            _centerFrequency = centerFrequency;
            _spanFrequency = spanFrequency;
            _autoFrequency = autoFrequency;
        }

        public QFactorSettings()
        {
            //_algorithm = Algorithms.Algorithm3dB;
            //_algorithm = Algorithms.AlgorithmLorenz;
            _algorithm = Algorithms.AlgorithmLeastSquareLorenz;
            _centerFrequency = 4500;
            _spanFrequency = 100;
            _autoFrequency = true;
        }

        public Algorithms Algorithm
        {
            get { return _algorithm; }
            set { _algorithm = value;}
        }

        public double CenterFrequency
        {
            get { return _centerFrequency; }
            set { _centerFrequency = value; }
        }

        public double SpanFrequency
        {
            get { return _spanFrequency; }
            set { _spanFrequency = value; }
        }

        public bool AutoFrequency
        {
            get { return _autoFrequency; }
            set { _autoFrequency = value; }
        }

    }
}
