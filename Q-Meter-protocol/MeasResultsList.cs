using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace Resonator
{
    public class MeasResultsList
    {
        private List<PointPair> pointList = new List<PointPair>();
        private int nextResultIdx;
        private string dataType = "#";  // used by QFactorAccuracyTest

        const int AVERAGE_STEP_N = 10;  // number of points to calculate average frequency step between points


        public void addDataPoint(MeasPacket mp)
        {
            pointList.Add(new PointPair(mp.Frequency, mp.Adc));
        }

        public void addFreqPower(double frequency, double power)
        {
            pointList.Add(new PointPair(frequency, power));
        }

        public void readFromS2P(string filename)
        {
            SimpleS2pReader ssr = new SimpleS2pReader();
            ssr.readFile(filename);
            pointList = ssr.getS21Module();
        }

        public double MinimumFrequency
        {
            get
            {
                return pointList[0].frequency;
            }
        }

        public double MaximumFrequency
        {
            get
            {
                return pointList[pointList.Count - 1].frequency;
            }
        }

        public void makeFromPointListCopy(List<PointPair> pointPairList)
        {
            pointList.Clear();
            for (int i=0; i<pointPairList.Count; i++)
            {
                pointList.Add(pointPairList[i]);
            }
        }

        public List<PointPair> getPointList()
        {
            return pointList;
        }

        public int Length => pointList.Count;

        public PointPair this[int index] => pointList[index];

        public void clear()
        {
            pointList.Clear();
        }

        // Resets to zero the pointer which indicates the next result which should be returned
        public void nextResultReset()
        {
            nextResultIdx = 0;
        }

        // Returns a point-pair object indicated by nextResultIdx
        public PointPair getNextResult()
        {
            if (nextResultIdx < pointList.Count)
                return pointList[nextResultIdx];
            else
                return null;
        }

        // Previous result has been taken into calculation of average result, so next point can be returned next time
        public void resultAccepted()
        {
            nextResultIdx++;
        }

        // Returns number of elements in pointList
        public int Count
        {
            get { return pointList.Count; }
        }

        public string DataType
        {
            get { return dataType; }
            set { dataType = value.Trim(); }
        }

        public PointPair getPeak()
        {
            if (pointList.Count == 0)
            {
                throw new InvalidOperationException("Empty list");
            }
            double maxGain = double.MinValue;
            int idx = 0;
            for (int i=0; i<pointList.Count; i++)
            {
                PointPair pp = pointList[i];
                if (pp.gain > maxGain)
                {
                    maxGain = pp.gain;
                    idx = i;
                }
            }
            return pointList[idx];
        }

        public void saveToCSV(string filename)
        {
            //make header srting
            StringBuilder result = new StringBuilder();
            result.Append("# Frequency [MHz]; Measured Power [dBm]");
            result.AppendLine();

            for (int i = 0; i < pointList.Count; i++)
            {
                result.Append(pointList[i].frequency.ToString(CultureInfo.InvariantCulture.NumberFormat));
                result.Append(';');
                result.Append(pointList[i].gain.ToString(CultureInfo.InvariantCulture.NumberFormat));
                result.AppendLine();
            }

            File.WriteAllText(filename, result.ToString());
        }

        public double calculateAverageStep()
        {
            int i=1;
            double avrStep;
            double sum = 0;
            while (i < pointList.Count && i <= AVERAGE_STEP_N)
            {
                sum += pointList[i].frequency - pointList[i - 1].frequency;
                i++;
            }

            avrStep = sum / (i - 1);
            return avrStep;
        }
    }
}
