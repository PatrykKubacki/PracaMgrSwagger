using QFactorCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class ChartData
    {
        public IEnumerable<Point> Points { get; set; }
        public IEnumerable<Maximum> Maximums { get; set; }
        public int MinimumPointValue => Points.Count() > 1 ? (int)Points.Min(p => p.Y) : 0;
        public IEnumerable<IEnumerable<Point>> GroupsOfPoints { get; set; }
        public IEnumerable<QFactorResult> QFactorResults { get; set; }

        public IEnumerable<IEnumerable<Point>> LorenzeCurves { get; set; }

        public IEnumerable<FitCurve> FitCurves { get; set; }

        public int MeasuredPointsPerSecond { get; set; }
    }

    public class Maximum
    {
        public double Frequency { get; set; }
        public double Value { get; set; }
    }
}
