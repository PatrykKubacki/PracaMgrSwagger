using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class FitCurve
    {
        public IEnumerable<Point> Points { get; set; }
        public bool IsFitError { get; set; }
    }
}
