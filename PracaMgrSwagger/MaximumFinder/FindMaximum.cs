using PracaMgrSwagger.Models;
using QFactorCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.MaximumFinder
{
    static public class FindMaximum
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


        static public List<Point> GetLorenzeCureve(IEnumerable<Point> points, QFactorResult qFactorResult)
        {
            var result = new List<Point>();
            foreach (var point in points)
            {
                var y = QFactorCalculator.QFactorCalculator.CalcLorenzeCurve(
                    point.Y,
                    point.X,
                    qFactorResult);

                result.Add(new Point { X = point.X, Y = y });
            };

            result = FilterLorenzeCurve(result);

            return result;
        }

        static List<Point> FilterLorenzeCurve(IEnumerable<Point> points)
        {
            var max = points.Max();
            var result = points.SkipWhile(x => x.X <= (max.X - 10))
                               .TakeWhile(x => x.X <= (max.X + 10))
                               .ToList();

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
            return result;
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
