using PracaMgrSwagger.Models;
using QFactorCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Helpers
{
    public static class MaximumHelper
    {
        static readonly int _limitParameter = -75;

        static public IEnumerable<Maximum> GetMaximumGroups(IEnumerable<Point> points)
        {
            var result = new List<Maximum>();
            var pointsOverLimit = SplitPoint(points);
            if (!pointsOverLimit.Any())
                return result;

            result = GetMaximums(pointsOverLimit);

            return result;
        }

        static List<List<Point>> SplitPoint(IEnumerable<Point> points)
        {
            var result = new List<List<Point>>();
            var templist = new List<Point>();
            foreach (var point in points)
            {
                if (point.Y > _limitParameter)
                {
                    templist.Add(point);
                    if (point.X == points.Last().X)
                        result.Add(templist);
                }
                else if (templist.Any())
                {
                    result.Add(templist);
                    templist = new List<Point>();
                }
            }
            return result;
        }

        static List<Maximum> GetMaximums(List<List<Point>> groupOfPoints)
        {
            var result = new List<Maximum>();
            foreach (var points in groupOfPoints)
            {
                var maxPoint = points.Max();
                if (maxPoint != null && maxPoint.Y > (_limitParameter+3))
                    result.Add(new Maximum {Frequency = maxPoint.X, Value = maxPoint.Y });
            }

            return result.Count < 10 ? result : new List<Maximum>();
        }

        static public IEnumerable<IEnumerable<Point>> GetGroupOfMaximumsPoints(IEnumerable<Point> points, IEnumerable<Maximum> maximums)
        {
            var result = new List<List<Point>>();
            var pointsOverLimit = SplitPoint(points);
            if (!pointsOverLimit.Any())
                return result;

            foreach (var maximum in maximums)
            {
                var point = points.FirstOrDefault(x => x.X == maximum.Frequency);
                var group = pointsOverLimit.FirstOrDefault(x => x.Any(y => y.X == maximum.Frequency));
                result.Add(group);
            }


            return result;
        }
    }
}
