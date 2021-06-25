using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class ChartHubConnections
    {
        public ChartHubParameters Connection { get; set; }

        public ChartHubConnections()
        {
            Connection = new ChartHubParameters();
        }

        public void SetStartFrequency (double value)
        {
            if (Connection != null)
                Connection.StartFrequency = value;
        }

        public void SetStopFrequency(double value)
        {
            if (Connection != null)
                Connection.StopFrequency = value;
        }

        public void SetStartStopFrequency(double start, double stop)
        {
            if (Connection != null)
            {
                Connection.StartFrequency = Math.Round(start);
                Connection.StopFrequency = Math.Round(stop);
                Connection.PointsOnScreen = 0;
            }
        }

        public void SetHubParameters(double start, double stop, int points)
        {
            if (Connection != null)
            {
                Connection.StartFrequency = Math.Round(start);
                Connection.StopFrequency = Math.Round(stop);
                Connection.PointsOnScreen = points;
            }
        }

        public void SetPointsOnScreen(int value)
        {
            if (Connection != null)
                Connection.PointsOnScreen = value;
        }

        public void SetIsObjectInside(bool value)
        {
            if (Connection != null)
                Connection.IsObjectInside = value;
        }

        public void UnZoomFull()
        {
            if (Connection != null)
                Connection.StartFrequency = 0;
                Connection.StopFrequency = 0;
                Connection.PointsOnScreen = 0;
        }
    }

    public class ChartHubParameters
    {
        public double StartFrequency { get; set; }
        public double StopFrequency { get; set; }
        public int PointsOnScreen { get; set; }
        public bool IsObjectInside { get; set; }
        public int Step { get; set; }
    }
}
