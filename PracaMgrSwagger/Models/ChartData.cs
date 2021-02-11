using PracaMgrSwagger.QFactorCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class ChartData
    {
        public IEnumerable<Point> Points { get; set; }
        public double StartFrequency { get; set; }
        public double StopFrequency { get; set; }
        public int PointsOnScreen { get; set; }
        public QFactorResult QFactorResult { get; set; }

    }
}
