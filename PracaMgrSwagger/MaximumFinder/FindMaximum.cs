using PracaMgrSwagger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.MaximumFinder
{
    static public class FindMaximum
    {
        static readonly int _limitParameter = -75;

        static public IEnumerable<Point> GetSmoothChart(IEnumerable<Point> points, ChartHubParameters hubParameters)
        {
            if(hubParameters == null)
                return points;

            var pointsOverLimit = SplitPoint(points);
            if (!pointsOverLimit.Any())
                return points;

            var firstPointOverLimit = pointsOverLimit.FirstOrDefault().FirstOrDefault();
            var indexOf = points.ToList().IndexOf(firstPointOverLimit);
            foreach (var groupOfPoint in pointsOverLimit)
            {
                foreach (var point in groupOfPoint)
                {
                    double k =  hubParameters.Step;
                    var value = hubParameters.Step == 0 
                        ? point.Y 
                        : (point.Y / k) -74d +(74 / k);
                    point.Y = value;
                    point.X -= k;
                }
            }
            var result = points.ToList();
            for (int i = (indexOf + 250); i > 0; i--)
                if (i<= points.Count() && points.ToArray()[i].X >= points.ToArray()[indexOf].X)
                    result.RemoveAt(i);

            return result;
        }

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
            return result;
        }

    }
}
