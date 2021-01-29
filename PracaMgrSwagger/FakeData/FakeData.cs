using PracaMgrSwagger.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.FakeDater
{
    static public class FakeData
    {
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

        public static ChartData GetChartData()
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

            var result = new ChartData
            {
                StartFrequency = points.First().X,
                StopFrequency = points.Last().X,
                PointsOnScreen = points.Count(),
                Points = points,
            };

            return result;
        }
    }
}
