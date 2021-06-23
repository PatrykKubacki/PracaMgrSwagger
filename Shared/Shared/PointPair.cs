using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class PointPair
    {
        public double frequency;
        public double gain;


        public PointPair()
        {
            frequency = gain = 0.0;
        }

        public PointPair(double x, double y)
        {
            this.frequency = x;
            this.gain = y;
        }
    }


    public class PointPairInt
    {
        int i0;
        int i1;


        public PointPairInt(int i0, int i1)
        {
            this.i0 = i0;
            this.i1 = i1;
        }

        public PointPairInt()
        {
            this.i0 = int.MaxValue;
            this.i1 = int.MaxValue;
        }

        public int I0
        {
            get { return i0; }
            set { i0 = value; }
        }

        public int I1
        {
            get { return i1; }
            set { i1 = value; }
        }

    }

}
