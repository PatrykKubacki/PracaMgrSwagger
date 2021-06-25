using PracaMgrSwagger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Helpers
{
    public static class FitErrorCurveHelper
    {
        public static IEnumerable<FitCurve> GetFitCurves(IEnumerable<Point> points, IEnumerable<IEnumerable<Point>> lorenzeCurves)
        {
            List<FitCurve> result = new();

            foreach (var lorenzeCurve in lorenzeCurves)
            {
                List<Point> fitCurve = new();

                foreach (var lorenzeCurvePoint in lorenzeCurve)
                {
                    Point point = points.FirstOrDefault(x => x.X == lorenzeCurvePoint.X);
                    if (point == null)
                        continue;

                    var y = lorenzeCurvePoint.Y - point.Y;
                    Point fitCurvePoint = new() { X = point.X, Y = y };
                    fitCurve.Add(fitCurvePoint);
                }
                var isFitError = IsFitError(fitCurve);
                FitCurve fitCurveResult = new() { Points = fitCurve, IsFitError = isFitError };
                result.Add(fitCurveResult);
            }

            return result;
        }
        static bool IsFitError(IEnumerable<Point> fitCurve)
        {
            double max = fitCurve.Max(x => Math.Abs(x.Y));
            return max > 1 ? true : false;
        }
    }
}
