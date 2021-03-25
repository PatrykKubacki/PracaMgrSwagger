using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class ChartHubConnections
    {
        public Dictionary<string, ChartHubParameters> Connections { get; set; }

        public ChartHubConnections()
        {
            Connections = new Dictionary<string, ChartHubParameters>();
        }

        public void SetStartFrequency (string connectionId, double value)
        {
            if (Connections != null)
                if (Connections.Any(x => x.Key == connectionId))
                    Connections[connectionId].StartFrequency = value;
                else
                    Connections.Add(connectionId, new ChartHubParameters { StartFrequency = value });
        }

        public void SetStopFrequency(string connectionId, double value)
        {
            if (Connections != null)
                if (Connections.Any(x => x.Key == connectionId))
                    Connections[connectionId].StopFrequency = value;
                else
                    Connections.Add(connectionId, new ChartHubParameters { StopFrequency = value });
        }

        public void SetStartStopFrequency(string connectionId, double start, double stop)
        {
            if (Connections != null)
                if (Connections.Any(x => x.Key == connectionId))
                {
                    Connections[connectionId].StartFrequency = Math.Round(start);
                    Connections[connectionId].StopFrequency = Math.Round(stop);
                    Connections[connectionId].PointsOnScreen = 0;
                }
            else
                Connections.Add(connectionId, new ChartHubParameters { StartFrequency = start, StopFrequency = stop });
        }

        public void SetPointsOnScreen(string connectionId, int value)
        {
            if (Connections != null)
                if (Connections.Any(x => x.Key == connectionId))
                    Connections[connectionId].PointsOnScreen = value;
                else
                    Connections.Add(connectionId, new ChartHubParameters { PointsOnScreen = value });
        }

        public void SetChartHubConnection(string connectionId)
        {
            if (Connections != null &&
                (!Connections.Any() || Connections.Any(x => x.Key != connectionId)) )
                Connections.Add(connectionId, new ChartHubParameters());
        }

        public void SetIsObjectInside(string connectionId, bool value)
        {
            if (Connections != null)
                if (Connections.Any(x => x.Key == connectionId))
                    Connections[connectionId].IsObjectInside = value;
                else
                    Connections.Add(connectionId, new ChartHubParameters { IsObjectInside = value });
        }

        public void UnZoomFull(string connectionId)
        {
            if (Connections != null)
                if (Connections.Any(x => x.Key == connectionId))
                {
                    Connections[connectionId].StartFrequency = 0;
                    Connections[connectionId].StopFrequency = 0;
                    Connections[connectionId].PointsOnScreen = 0;
                }
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
