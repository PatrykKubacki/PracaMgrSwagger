using QMeterProtocol;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class MeasResultsList
    {
        private List<PointPair> pointList = new List<PointPair>();
        private int nextResultIdx;
        private string dataType = "#";  // used by QFactorAccuracyTest

        const int _limitParameter = -75;
        const int AVERAGE_STEP_N = 10;  // number of points to calculate average frequency step between points


        public void addFreqPower(double frequency, double power)
        {
            pointList.Add(new PointPair(frequency, power));
        }

        public void addDataPoint(MeasPacket mp)
        {
            pointList.Add(new PointPair(mp.Frequency, mp.Adc));
        }


        public void readFromS2P(string filename)
        {
            SimpleS2pReader ssr = new SimpleS2pReader();
            ssr.readFile(filename);
            pointList = ssr.getS21Module();
        }

        public void FilterPointList(double start, double stop)
        {
            IEnumerable<PointPair> filteredPointList = start == 0 
                ? pointList 
                : pointList.SkipWhile(point => point.frequency <= start);

            filteredPointList = stop == 0 
                ? filteredPointList 
                : filteredPointList.TakeWhile(point => point.frequency <= stop);

            pointList = filteredPointList.ToList();

        }

        public void TransformPointList(int step)
        {
            var pointsOverLimit = SplitPoint(pointList);
            if (!pointsOverLimit.Any())
                return ;

            var firstPointOverLimit = pointsOverLimit.FirstOrDefault().FirstOrDefault();
            var indexOf = pointList.ToList().IndexOf(firstPointOverLimit);
            foreach (var groupOfPoint in pointsOverLimit)
            {
                foreach (var point in groupOfPoint)
                {
                    double k = step;
                    var value = step == 0
                        ? point.gain
                        : (point.gain / k) - 74d + (74 / k);
                    point.gain = value;
                    point.frequency -= k;
                }
            }

            var result = pointList.ToList();
            for (int i = (indexOf + 250); i > 0; i--)
                if (i <= pointList.Count() && pointList.ToArray()[i].frequency >= pointList.ToArray()[indexOf].frequency)
                    result.RemoveAt(i);

            pointList = result;
        }

        private List<List<PointPair>> SplitPoint(List<PointPair> pointList)
        {
            var result = new List<List<PointPair>>();
            var templist = new List<PointPair>();
            foreach (var point in pointList)
            {
                if (point.gain > _limitParameter)
                {
                    templist.Add(point);
                    if (point.frequency == pointList.Last().frequency)
                        result.Add(templist);
                }
                else if (templist.Any())
                {
                    result.Add(templist);
                    templist = new List<PointPair>();
                }
            }

            return result;
        }

        public void CutPoints(int pointsOnScreen)
        {
            var pointsCount = pointList.Count();
            if (pointsOnScreen != 0 && pointsOnScreen < pointsCount)
            {
                var pointToCutCount = pointsCount - pointsOnScreen;
                var skipEvery = Math.Round((double)pointsCount / (double)pointToCutCount);

                pointList = skipEvery > 1
                        ? pointList.SkipWhile((point, index) => index % skipEvery == 0)
                                   .ToList()
                        : GetFilteredPoints(pointList, pointToCutCount);
            }
        }

        private List<PointPair> GetFilteredPoints(List<PointPair> pointList, int howManyCut)
        {
            var result = pointList.ToList();

            while (howManyCut > 0)
            {
                var totalPoints = result.Count();
                var cuted = totalPoints / 2;
                result = result.Where((p, index) => index % 2 == 0).ToList();
                howManyCut -= cuted;
            }

            return result;
        }

        public IEnumerable<PointPair> GetPointsInRange(double start, double stop)
            => pointList.Where(x => x.frequency >= start && x.frequency <= stop);

        public double MinimumFrequency {
            get {
                return pointList[0].frequency;
            }
        }

        public double MaximumFrequency {
            get {
                return pointList[pointList.Count - 1].frequency;
            }
        }

        public void makeFromPointListCopy(List<PointPair> pointPairList)
        {
            pointList.Clear();
            for (int i = 0; i < pointPairList.Count; i++)
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
        public int Count {
            get { return pointList.Count; }
        }

        public string DataType {
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
            for (int i = 0; i < pointList.Count; i++)
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
