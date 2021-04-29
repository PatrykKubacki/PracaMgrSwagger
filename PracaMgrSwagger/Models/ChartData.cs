﻿using QFactorCalculator;
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
        public IEnumerable<Maximum> Maximums { get; set; }
        public int MinimumPointValue => (int)Points.Min(p => p.Y);

        public IEnumerable<IEnumerable<Point>> GroupsOfPoints { get; set; }
        public IEnumerable<QFactorResult> QFactorResults { get; set; }
        public IEnumerable<IEnumerable<Point>> LorenzeCurves { get; set; }

        public IEnumerable<FitCurve> FitCurves { get; set; }
    }

    public class Maximum
    {
        public double Frequency { get; set; }
        public double Value { get; set; }
    }
}
