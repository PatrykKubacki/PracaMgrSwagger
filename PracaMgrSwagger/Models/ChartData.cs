using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class ChartData
    {
        public IEnumerable<Point> Points { get; set; }
        public decimal StartFrequency { get; set; }
        public decimal StopFrequency { get; set; }
        public int PointsOnScreen { get; set; }
    }
}
