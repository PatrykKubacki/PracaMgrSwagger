using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaMgrSwagger.QFactorCalculator
{
    class SimpleS2pReader
    {
        List<PointPair> s21module = new List<PointPair>();

        public void readFile(string filename)
        {
            double frequency;
            double sparam;
            var random = new Random();
            
            Console.WriteLine("Reading file: " + filename);

            string[] lines = System.IO.File.ReadAllLines(filename);

            foreach (string line in lines)
            {
                if (line[0] == '!' || line[0] == '#')     // skip comments, do not recognize format (TODO: recognize file format)
                    continue;
                string[] values = line.Split(' ');
                double.TryParse(values[0].Replace(".", ","), out frequency);
                sparam = double.Parse(values[3], CultureInfo.InvariantCulture);
                s21module.Add(new PointPair(frequency, (20 * Math.Log10(sparam))+ random.NextDouble()));
            }
            Console.WriteLine("Number of datapoints: " + s21module.Count.ToString());
        }

        public List<PointPair> getS21Module()
        {
            return this.s21module;
        }
    }
}
