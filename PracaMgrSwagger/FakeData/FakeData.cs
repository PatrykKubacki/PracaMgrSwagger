using PracaMgrSwagger.FakeData;
using PracaMgrSwagger.MaximumFinder;
using PracaMgrSwagger.Models;
using QFactorCalculator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PracaMgrSwagger.FakeDater
{
    static public class FakeData
    {
        //DataSourceFromFiles _dataSourceFromFiles { get; set; }

        public static ResonatorParameters GetFakeEmptyResonator()
        {
            var random = new Random();
            var qFactor = random.Next(9500, 9600);
            var centerFrequency = random.Next(5123, 5200);
            var peakTransmittance = random.Next(-40, -33);
            return new ResonatorParameters(qFactor, centerFrequency, peakTransmittance);
        }
        public static ChartData GetFakeChartData()
        {
            var random = new Random();
            var pointsOnScreen = 4000;
            var result = new ChartData
            {
                StartFrequency = 0,
                StopFrequency = 0,
                PointsOnScreen = pointsOnScreen,
                Points = new List<Point>(),
            };


            for (var i = -2000; i < 2000; i += 1)
            {
                var y = Math.Round(Convert.ToDouble(random.Next(1, 10) * 10 - 5));
                var point = new Point { X = Convert.ToDouble(i), Y = y };
                (result.Points as List<Point>).Add(point);

            };

            return result;
        }

        public static ChartData GetChartDataFromS2PFile(DataSourceFromFiles dataSourceFromFiles, ChartHubParameters hubParameters = null)
        {
            var random = new Random();
            var qFactorSettings = new QFactorSettings();
            var measResultsList = dataSourceFromFiles.GetMeasResultsList(hubParameters);
            //var pointList = measResultsList.getPointList();

            //pointList.ForEach(x => x.gain += random.NextDouble());
            //measResultsList = new MeasResultsList(pointList);

            var qFactorCalculator = new QFactorCalculator.QFactorCalculator(measResultsList, qFactorSettings);
            QFactorResult qFactorResult = qFactorCalculator.calculateQFactor();
            var points = measResultsList.getPointList()
                                        .Select(point => new Point{ X = point.frequency/1000000, Y = point.gain });

            ChartData result;
            if (hubParameters != null)
            {
                result = new ChartData();
                if (hubParameters.StartFrequency != 0)
                {
                    points = points.SkipWhile(point => point.X <= hubParameters.StartFrequency)
                                   .ToList();

                    result.StartFrequency = hubParameters.StartFrequency;
                }
                else
                    result.StartFrequency = points.First().X;

                if (hubParameters.StopFrequency != 0)
                {
                    points = points.TakeWhile(point => point.X <= hubParameters.StopFrequency)
                                   .ToList();

                    result.StopFrequency = hubParameters.StopFrequency;
                }
                else
                    result.StopFrequency = points.Last().X;


                var pointsCount = points.Count();
                if (hubParameters.PointsOnScreen != 0 && hubParameters.PointsOnScreen < pointsCount)
                {
                    var pointToCutCount = pointsCount - hubParameters.PointsOnScreen;
                    var skipEvery = Math.Round((double)pointsCount / (double)pointToCutCount);

                    points = skipEvery > 1
                        ? points.SkipWhile((point, index) => index % skipEvery == 0)
                                .ToList()
                        : GetFilteredPoints(points, pointToCutCount);


                    result.PointsOnScreen = hubParameters.PointsOnScreen;
                }
                else
                    result.PointsOnScreen = points.Count();

                result.Maximums = FindMaximum.GetMaximumGroups(points);

                if (hubParameters.IsObjectInside)
                {
                    if (hubParameters.Step != 5)
                    {
                         Thread.Sleep(100);
                         points = FindMaximum.GetSmoothChart(points, hubParameters);
                         hubParameters.Step++;
                    } 
                    else
                        points = FindMaximum.GetSmoothChart(points, hubParameters);
                }
                else
                {
                    if (hubParameters.Step != 0)
                    {
                        Thread.Sleep(100);
                        points = FindMaximum.GetSmoothChart(points, hubParameters);
                        hubParameters.Step--;
                    }
                }
            }
            else
            {
                result = new ChartData()
                {
                    PointsOnScreen = measResultsList.Count,
                    Points = points,
                    StartFrequency = Math.Round(points.First().X, 2),
                    StopFrequency = Math.Round(points.Last().X, 2),
                    QFactorResult = qFactorResult,
                };
            }

            //result.PointsOnScreen = points.Count();
            result.Points = points;
            result.QFactorResult = qFactorResult;
            result.LorenzeCurve = FindMaximum.GetLorenzeCureve(points, qFactorResult);

            //result.Maximums = FindMaximum.GetMaximumGroups(points);
            //result = new ChartData()
            //{
            //    PointsOnScreen = measResultsList.Count,
            //    Points = points,
            //    StartFrequency = Math.Round(points.First().X,2),
            //    StopFrequency = Math.Round(points.Last().X, 2),
            //    QFactorResult = qFactorResult,
            //};
            return result;
        }

        static List<Point> GetFilteredPoints(IEnumerable<Point> points, int howManyCut)
        {
            var result = points.ToList();

            while(howManyCut > 0)
            {
                var totalPoints = result.Count();
                var cuted = totalPoints / 2;
                result = result.Where((p, index) => index % 2 == 0).ToList();
                howManyCut -= cuted;
            }

            return result;
        }

        public static ChartData GetChartData(ChartHubParameters hubParameters = null)
        {
            var random = new Random();
            var points = new List<Point>();
            //var fileName = "spdr 5 close.csv";
            var fileName = "spdr 5g wide.csv";
            var path = Path.Combine(Environment.CurrentDirectory, @"Assets\", fileName);
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    var point = new Point
                    {
                        //X = double.Parse(values[0].Replace(".",",")) / 1000000,
                        //Y = double.Parse(values[3].Replace(".", ",")),
                        X = double.Parse(values[0].Replace(".", ",")) * 1000,
                        Y = double.Parse(values[1].Replace(".", ",")) + random.NextDouble(),
                    };

                    points.Add(point);
                }
            }
            var result = new ChartData();

            if (hubParameters != null)
            {
                if (hubParameters.StartFrequency != 0)
                {
                    points = points.SkipWhile(point => point.X <= hubParameters.StartFrequency)
                                   .ToList();

                    result.StartFrequency = hubParameters.StartFrequency;
                }
                else
                    result.StartFrequency = points.First().X;

                if (hubParameters.StopFrequency != 0)
                {
                    points = points.TakeWhile(point => point.X <= hubParameters.StopFrequency)
                                   .ToList();

                    result.StopFrequency = hubParameters.StopFrequency;
                }
                else
                    result.StopFrequency = points.Last().X;
            }
            else
            {
                result = new ChartData
                {
                    StartFrequency = points.First().X,
                    StopFrequency = points.Last().X,
                    PointsOnScreen = points.Count(),
                    Points = points,
                };
            }

            //result.StopFrequency = points.Last().X;
            result.PointsOnScreen = points.Count();
            result.Points = points;

            return result;
        }
    }
}
