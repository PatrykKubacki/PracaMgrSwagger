using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace QMeterProtocol
{
    public class PerformanceCounter
    {
        DateTime startTime;
        int numberOfPoints=0;
        int scanNumber=0;
        const int AVERAGING = 10;
        double measRate = 0.0f;      // expresses a number of points per second measured by hardware

        public PerformanceCounter()
        {
            startTime = DateTime.Now;
        }

        public void storePointsFromOneScan(int points)
        {
            numberOfPoints += points;
            scanNumber++;
            if (scanNumber > AVERAGING)
                calculateMeasRate();
        }

        void calculateMeasRate()
        {
            TimeSpan ts;

            ts = DateTime.Now - startTime;
            measRate = ((double)numberOfPoints / ts.TotalMilliseconds) * 1000;

            startTime = DateTime.Now;
            numberOfPoints = 0;
            scanNumber = 0;
        }

        public double MeasRate
        {
            get { return measRate; }
        }

    }
}
