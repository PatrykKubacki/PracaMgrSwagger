using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QFactorCalculator
{
    public class MeasResultsList
    {
        private List<PointPair> pointList = new List<PointPair>();
        private int nextResultIdx;
        private string dataType = "#";  // used by QFactorAccuracyTest

        const int AVERAGE_STEP_N = 10;  // number of points to calculate average frequency step between points


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

        public List<PointPair> getPointList()
        {
            return pointList;
        }

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
        public int Count {
            get { return pointList.Count; }
        }

        public string DataType {
            get { return dataType; }
            set { dataType = value.Trim(); }
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
            int i = 1;
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
