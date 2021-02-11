using System;
using System.Collections.Generic;
using System.Text;

namespace PracaMgrSwagger.QFactorCalculator
{
    public class QFactorSettings
    {
        public enum Algorithms { Algorithm3dB, Algorithm10dB, AlgorithmIterativeLorenz, AlgorithmLeastSquareLorenz };

        private Algorithms algorithm;       // Algorithm used to calculate the Q-Factor
        private double centerFrequency;     // Center frequency from which we are starting looking for the transmittance peak
        private double spanFrequency;       // Range of frequencies to look for the transmittance peak
        private bool autoFrequency;         // Should be set to TRUE when center- and spanFrequency are not known

        public QFactorSettings(Algorithms alg, double centerFrequency, double spanFrequency, bool autoFrequency)
        {
            this.algorithm = alg;
            this.centerFrequency = centerFrequency;
            this.spanFrequency = spanFrequency;
            this.autoFrequency = autoFrequency;
        }

        public QFactorSettings()
        {
            //algorithm = Algorithms.Algorithm3dB;
            //algorithm = Algorithms.AlgorithmLorenz;
            algorithm = Algorithms.AlgorithmLeastSquareLorenz;
            centerFrequency = 4500;
            spanFrequency = 100;
            autoFrequency = true;
        }

        public Algorithms Algorithm
        {
            get { return algorithm; }
            set { algorithm = value;}
        }

        public double CenterFrequency
        {
            get { return centerFrequency; }
            set { centerFrequency = value; }
        }

        public double SpanFrequency
        {
            get { return spanFrequency; }
            set { spanFrequency = value; }
        }

        public bool AutoFrequency
        {
            get { return autoFrequency; }
            set { autoFrequency = value; }
        }

    }
}
