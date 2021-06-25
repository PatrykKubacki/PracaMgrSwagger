using PracaMgrSwagger.Models;
using QFactorCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Helpers
{
    public static class LorenzeCurveHelper
    {
        static public IEnumerable<IEnumerable<Point>> GetLorenzeCurves(IEnumerable<IEnumerable<Point>> groupOfPoints, IEnumerable<QFactorResult> qFactorResults)
        {
            List<List<Point>> result = new();

            if (!groupOfPoints.Any() || !qFactorResults.Any())
                return result;

            int countGroupOfPoints = groupOfPoints.Count();
            int countQFactorResults = qFactorResults.Count();
            var countOfLorenze = countGroupOfPoints == countQFactorResults
                ? countGroupOfPoints
                : countQFactorResults;

            if (countOfLorenze > 10)
                return result;

            for (int i = 0; i <= (countOfLorenze - 1); i++)
            {
                IEnumerable<Point> points = groupOfPoints.ToArray()[i];
                QFactorResult qFactorResult = qFactorResults.ToArray()[i];

                List<Point> lorenzeCurve = GetLorenzeCureve(points, qFactorResult).ToList();

                result.Add(lorenzeCurve);
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
            var result = points.SkipWhile(x => x.X <= (max.X - 100))
                               .TakeWhile(x => x.X <= (max.X + 100))
                               .ToList();

            return result;
        }
    }
}
