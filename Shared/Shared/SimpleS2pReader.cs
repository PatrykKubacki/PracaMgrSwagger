using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    class SimpleS2pReader
    {
        List<PointPair> s21module = new List<PointPair>();

        public void readFile(string filename)
        {
            double frequency;
            double sparam;
            
            Console.WriteLine("Reading file: " + filename);

            string[] lines = System.IO.File.ReadAllLines(filename);

            foreach (string line in lines)
            {
                if (line[0] == '!' || line[0] == '#')     // skip comments, do not recognize format (TODO: recognize file format)
                    continue;
                string[] values = line.Split(' ');
                double.TryParse(values[0], out frequency);
                double.TryParse(values[3], out sparam);
                s21module.Add(new PointPair(frequency, 20 * Math.Log10(sparam)));
            }
            Console.WriteLine("Number of datapoints: " + s21module.Count.ToString());
        }

        public List<PointPair> getS21Module()
        {
            return this.s21module;
        }
    }
}
