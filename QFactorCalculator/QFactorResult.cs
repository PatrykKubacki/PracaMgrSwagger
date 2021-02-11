using System;
using System.Collections.Generic;
using System.Text;

namespace QFactorCalculator
{
    public class QFactorResult
    {
        double q_factor;            // calculated Q-Factor
        double centerFrequency;     // center frequency of calculated Q [MHz]
        double bandwidth;           // 3-db bandwidth
        double peakTransmittance;   // transmittance at center frequency of the resonator [dB]
        int numberOfPoints;         // Number of points used to calculate Q-factor
        bool uncalPll = false;      // when TRUE, warning is displayed due to PLL out-of-lock at some points
        bool uncalLevel = false;    // when TRUE, warning is displayed due to too low level of input signal
        bool uncalPoints = false;   // when TRUE, warning is displayed due to no enough points to calculate Q properly
        const int WARNING_LOW_POINTS = 25;
        protected string dataType = "#";  // used by QFactorAccuracyTest

        public QFactorResult(double q, double centerFrequency, double peakTransmittance, double bandwidth, int numberOfPoints)
        {
            q_factor = q;
            this.centerFrequency = centerFrequency;
            this.peakTransmittance = peakTransmittance;
            this.bandwidth = bandwidth;
            this.numberOfPoints = numberOfPoints;
            checkUncalPoints();
        }

        public QFactorResult(double centerFrequency, int numberOfPoints)
        {
            q_factor = 0;
            this.centerFrequency = centerFrequency;
            this.numberOfPoints = numberOfPoints;
            checkUncalPoints();
        }

        void checkUncalPoints()
        {
            if (numberOfPoints < WARNING_LOW_POINTS) uncalPoints = true;
        }

        public QFactorResult()
        {
            q_factor = 0;
            centerFrequency = 0;
        }

        public double Q_factor
        {
            get { return q_factor; }
            set { q_factor = value; }
        }

        public double CenterFrequency
        {
            get { return centerFrequency; }
            set { centerFrequency = value; }
        }

        public double Bandwidth
        {
            get { return bandwidth; }
            set { bandwidth = value; }
        }

        public int NumberOfPoints
        {
            get { return numberOfPoints; }
            set { numberOfPoints = value; checkUncalPoints(); }
        }

        public double PeakTransmittance
        {
            get { return peakTransmittance; }
            set { peakTransmittance = value; }
        }

        public bool UncalPll
        {
            get { return uncalPll; }
            set { uncalPll = value; }
        }

        public bool UncalPoints
        {
            get { return uncalPoints; }
            set { uncalPoints = value; }
        }

        public bool UncalLevel
        {
            get { return uncalLevel; }
            set { uncalLevel = value; }
        }

        public string DataType
        {
            get { return dataType; }
            set { dataType = value; }
        }

        public override string ToString()
        {
            string s = "\nCenter frequency: " + (CenterFrequency / 1e6).ToString() + " MHz\n" +
                        "Q-Factor: " + Q_factor +
                        "\nPeak transmittance: " + PeakTransmittance.ToString() + " dB\n" +
                        "Bandwidth: " + (Bandwidth / 1e3).ToString() + " kHz";
            return s;
        }
    }
}
