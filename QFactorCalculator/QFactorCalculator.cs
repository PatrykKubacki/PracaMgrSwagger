using System;
using System.Collections.Generic;
using System.Text;
using DotNetMatrix;

namespace QFactorCalculator
{
    public class QFactorCalculator
    {
        private List<PointPair> pointList;
        private QFactorSettings qFactorSettings;
        double[,] lorenzTable;
        int lorenz_N = 100;                 // number of samples at which calculation will proceed
        private enum Lorenz { LOR_FREQ, LOR_MEAS, LOR_CALC };


        public QFactorCalculator(MeasResultsList mrl, QFactorSettings qfs)
        {
            pointList = mrl.getPointList();
            qFactorSettings = qfs;
        }

        // Calculates the Q-Factor depending on the algorithm and settings saved in qFactorSettings object
        public QFactorResult calculateQFactor()
        {
            if (pointList.Count <= 3)
                throw new ApplicationException("Number of data points is too low.");

            QFactorResult qfr=null;

            switch (qFactorSettings.Algorithm)
            {
                case QFactorSettings.Algorithms.Algorithm3dB:       qfr = calculate3dB(); break;
                case QFactorSettings.Algorithms.Algorithm10dB:      qfr = calculate10dB(); break;
                case QFactorSettings.Algorithms.AlgorithmIterativeLorenz: qfr = null; break;         // this option is no longer in use
                case QFactorSettings.Algorithms.AlgorithmLeastSquareLorenz: qfr = calculateLeastSquareLorenz(); break;
            }

            return qfr;
        }

        // Calculates the Q-Factor by measuring the -3dB bandwidth of the filter
        private QFactorResult calculate3dB()
        {
            return calculateDb(3.0F);
        }

        // Calculates the Q-Factor by measuring the -10dB bandwidth of the filter
        private QFactorResult calculate10dB()
        {
            return calculateDb(10.0F);
        }

        // Generic method of Q-Factor calculation depending on the bandwidth and center frequency of the filter
        private QFactorResult calculateDb(double dB)
        {
            int center;
            QFactorResult qfr;
            PointPair bandwidth;

            qfr = new QFactorResult();

            center = findCenterFrequencyIndex();
            bandwidth = findBandwidth(center, dB);

            if (bandwidth.frequency == double.NaN || bandwidth.gain == double.NaN)
                return qfr;   // Q-Factor can't be calculated! 

            qfr.CenterFrequency = pointList[center].frequency;
            qfr.Bandwidth = bandwidth.gain - bandwidth.frequency;
            qfr.Q_factor = qfr.CenterFrequency / qfr.Bandwidth;
            qfr.PeakTransmittance = pointList[center].gain;
            qfr.NumberOfPoints = 3;

            return qfr;
        }

 

        private QFactorResult calculateLeastSquareLorenz()
        {
            int i, j=0;
            int f0_index;
            PointPairInt range;
            int n;

            // Preprocessing (removing data points which are far away from transmittance peak)
            f0_index = findCenterFrequencyIndex();
            if (f0_index < 0 || f0_index > pointList.Count)
                return new QFactorResult();   // return "no result" if center frequency not found.
            range = findBandwidthIndexes(f0_index, 3.0F);
            if (range.I0 == int.MaxValue)
                return new QFactorResult();   // return "no result" if center frequency not found.

            n = range.I1 - range.I0;
            if (n < 5) 
            {       // When we have very small amount of points, turn off Q-calculations completely - we don't want to have useless results!
                    // We need to return the center frequency which is already found.
                return new QFactorResult(pointList[f0_index].frequency, n);
            }

            GeneralMatrix X = new GeneralMatrix(n, 3);  // vandermonde matrix with measured frequency points
            GeneralMatrix Y = new GeneralMatrix(n, 3);  // vector with measured power levels
            for (i = 0; i < pointList.Count; i++)
            {
                if (i > range.I0 && i <= range.I1)
                {
                    X.SetElement(j, 0, Math.Pow(pointList[i].frequency, 2));
                    X.SetElement(j, 1, pointList[i].frequency);
                    X.SetElement(j, 2, 1);
                    Y.SetElement(j, 0, Math.Pow(10, -pointList[i].gain / 10));   // measured power is converted to linear form and inverted (1/x)
                    j++;
                }
            }
            QRDecomposition QR = new QRDecomposition(X);  
            GeneralMatrix beta = QR.Solve(Y);               // Linear regression used to find beta coefficients

            double A = beta.GetElement(0,0);
            double B = beta.GetElement(1,0);
            double C = beta.GetElement(2,0);

            double F0 = -B / (2*A);
            double Q = -B / (2*Math.Sqrt(4*A*C-B*B));
            double P0 = -1 / ((B*B)/(4*A)-C);
            P0 = 10*Math.Log10(P0);

            //return new QFactorResult(Q, F0, F0 - 0.5 * F0 / Q, F0 + 0.5 * F0 / Q, P0-3, P0-3);
            return new QFactorResult(Q, F0, P0, F0 / Q, n);
        }





        // Calculates value of Lorenz curve at given frequency in linear value
        // f, f0 [MHz]
        // Q - no dimensional
        // k - linear gain (|k|<1 for attenuation)
        private double lorenzCurve(double f, double Q, double f0, double k)
        {
            return k / Math.Sqrt(1 + Q * Q * Math.Pow((f / f0 - f0 / f), 2));
        }


        // Returns index of the datapoint with maximum transmittance of the filter (center frequency)
        private int findCenterFrequencyIndex()
        {
            double minFrequency;
            double maxFrequency;
            double centerPower;
            //double centerFrequency;
            int centerIndex;

            if (qFactorSettings.AutoFrequency)
            {
                minFrequency = pointList[0].frequency;
                maxFrequency = pointList[pointList.Count - 1].frequency;
            }
            else
            {
                minFrequency = qFactorSettings.CenterFrequency - qFactorSettings.SpanFrequency / 2;
                maxFrequency = qFactorSettings.CenterFrequency + qFactorSettings.SpanFrequency / 2;
            }

            centerPower = double.MinValue;
            //centerFrequency = minFrequency;

            int i = 0;
            centerIndex = -1;

            while (i < pointList.Count)
            {
                if (pointList[i].frequency < minFrequency) // is frequency of the datapoint over minFrequency limit?                
                    continue;                      // if not, go to next datapoint 

                if (pointList[i].frequency > maxFrequency) // is frequency below maxFrequency limit?
                    break;                         // if not, break the loop 

                if (pointList[i].gain > centerPower)  // we've found point with higher tranmittance
                {
                    centerPower = pointList[i].gain;
                    centerIndex = i;
                }
                i++;
            }

            return centerIndex;
        }

        // Finds indexes of samples which gives "-3dB" range 
        // Returns int.MaxValue if index cannot be found
        private PointPairInt findBandwidthIndexes(int center, double dB)
        {
            int i;
            PointPairInt ret = new PointPairInt();  // returned values

            if (center < 2 || center > pointList.Count - 3)  // There is no sense to find a bandwidth when center frequency is very close to one of the ends of datapoint set
            {
                return new PointPairInt(int.MaxValue, int.MaxValue);
            }
            dB = Math.Abs(dB);

            i = center;
            while (i < pointList.Count && (pointList[center].gain < pointList[i].gain + dB))
                i++;

            if (i == pointList.Count)
            {
                // ERROR occured! We've got and of data point list! Calculated BW will be too low.
                // Should exit here!
                return new PointPairInt(int.MaxValue, int.MaxValue);
            }

            // index i points to datapoint which is more than "3dB" far from transmittance peak
            //f2 = pointList[i - 1].X;
            ret.I1 = i;

            i = center;
            while (i >= 0 && (pointList[center].gain < pointList[i].gain + dB))
                i--;

            if (i == -1)
            {
                // ERROR occured! We've got and of data point list! Calculated BW will be too low.
                // Should exit here!
                return new PointPairInt(int.MaxValue, int.MaxValue);
            }

            // index i points to datapoint which is more than "3dB" far from transmittance peak
            //f1 = pointList[i + 1].X;

            ret.I0 = i;

            return ret;
        }
        

        // Calculates bandwidth depending on given center frequency index and relative power level at which BW should be calculated
        private PointPair findBandwidth(int center, double dB)
        {
            double f1, f2;
            PointPairInt indexes;  // indexes of the "-3dB" bounds

            indexes = findBandwidthIndexes(center, dB);

            // We have to check, whether or not valid indexes have been found
            if (indexes.I0 == int.MaxValue || indexes.I1 == int.MaxValue)
                return new PointPair(double.NaN, double.NaN);

            // We're looking for the frequency located between two datapoints: "i-1" (less than -3dB) and "i" (more than -3dB)
            f2 = linearInterpolation(pointList[center].gain - dB, pointList[indexes.I1 - 1].gain, pointList[indexes.I1 - 1].frequency, pointList[indexes.I1].gain, pointList[indexes.I1].frequency); 

            // We're looking for the frequency located between two datapoints: "i+1" (less than -3dB) and "i" (more than -3dB)
            f1 = linearInterpolation(pointList[center].gain - dB, pointList[indexes.I0 + 1].gain, pointList[indexes.I0 + 1].frequency, pointList[indexes.I0].gain, pointList[indexes.I0].frequency); 

            return new PointPair(f1, f2);
        }

        private double linearInterpolation(double x, double x0, double y0, double x1, double y1)
        {
            return y0 + (x - x0) * ((y1 - y0) / (x1 - x0));
        }
    }
}
