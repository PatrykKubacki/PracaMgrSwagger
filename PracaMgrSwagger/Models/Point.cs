using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class Point: IComparable
    {
        public double X { get; set; }
        public double Y { get; set; }

        public int CompareTo(object obj)
        {
            var anotherPoint = (Point)obj;
            if (Y> anotherPoint.Y)
                return 1;

            return Y < anotherPoint.Y ? -1 : 0;
        }

        public override string ToString()
            => $"X: {X} Y:{Y}";
    }
}
