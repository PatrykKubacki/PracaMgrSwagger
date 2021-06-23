using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PracaMgrSwagger.Hubs;
using PracaMgrSwagger.MaximumFinder;
using PracaMgrSwagger.Models;
using QFactorCalculator;
using QMeterProtocol;
using Shared;

namespace PracaMgrSwagger.QMeterProtocol
{
    public class QMeterProtocol
    {
        IHubContext<ChartHub> _chartHub;
        ChartHubConnections _chartHubConnections;
        ProtocolHandler _ph;
        MeasManager _measManager;
        MeasResultsList _mrl;
        bool _firstSetupDone;
        const int _oversampling = 50;
        double _minFrequency = 0;
        double _maxFrequency = 0;

        public ChartData ChartData { get; private set; }

        public QMeterProtocol(IHubContext<ChartHub> hub, ChartHubConnections connections)
        {
            ChartData = new ChartData() { QFactorResult = new QFactorResult() };
            _chartHub = hub;
            _chartHubConnections = connections;
            _measManager = new MeasManager();
            _ph = new ProtocolHandler(_measManager);
            _firstSetupDone = false;
        }

        public void Run()
        {
            while (true)
            {
                try
                {
                    if (_ph.HardwareIsConnected)
                    {
                        if(!_firstSetupDone)
                        {
                            _firstSetupDone = true;
                            _ph.cmdSetScanRange(_ph.QMeterMinFrequency * 1000, _ph.QMeterMaxFrequency * 1000, 8000, ProtocolHandler.MeasType.mtS21, _oversampling, 0);

                        }
                        else
                        {
                            if (_chartHubConnections != null && _chartHubConnections.Connection != null)
                            {
                                if (_minFrequency != _chartHubConnections.Connection.StartFrequency || _maxFrequency != _chartHubConnections.Connection.StopFrequency)
                                {
                                    _minFrequency = _chartHubConnections.Connection.StartFrequency;
                                    _maxFrequency = _chartHubConnections.Connection.StopFrequency;
                                    var start = Convert.ToInt32(_minFrequency) * 1000;
                                    var stop = Convert.ToInt32(_maxFrequency) * 1000;

                                    _ph.cmdSetScanRange(start, stop, 1000, ProtocolHandler.MeasType.mtS21, _oversampling, 0);
                                }
                            }
                        }
                    } 

                    _mrl = _measManager.getActualMeasResultsList();
                    if (_mrl != null)
                    {
                        ChartData = ConvertDeviceDataToResultChartData(_mrl);
                        NotifyChange();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"!!!!!!!!!!!!!!!!!!! Exception !!!!!!!!!!!!! {ex.Message}");
                }
            }
        }

        void NotifyChange()
            => _chartHub.Clients.All.SendAsync("SendChart", ChartData);
            

        ChartData ConvertDeviceDataToResultChartData(MeasResultsList measResultsList)
        {
            ChartData result = new();
            var pointList = measResultsList.getPointList();

            result.Points = pointList.Select(point => new Point { X = point.frequency, Y = point.gain })
                                         .ToList(); ;

            //result.Points = points;
            //result.PointsOnScreen = measResultsList.Count;
            //result.StartFrequency = Math.Round(points.First().X, 2);
            //result.StopFrequency = Math.Round(points.Last().X, 2);
            //result.Maximums = FindMaximum.GetMaximumGroups(points);
            //result.GroupsOfPoints = FindMaximum.GetGroupOfMaximumsPoints(points, result.Maximums);
            //result.QFactorResults = GetQFactorResults(measResultsList, result.GroupsOfPoints);
            //result.LorenzeCurves = GetLorenzeCurves(result.GroupsOfPoints, result.QFactorResults);
            //result.FitCurves = GetFitCurves(points, result.LorenzeCurves);

            //result.QFactorResults = new List<QFactorResult>() { CalcualteQFactor(measResultsList) };

            return result;
        }

        QFactorResult CalcualteQFactor(MeasResultsList measResultsList)
        {
            QFactorSettings qFactorSettings = new();
            QFactorCalculator.QFactorCalculator qFactorCalculator = new (measResultsList, qFactorSettings);
            QFactorResult qFactorResult = qFactorCalculator.calculateQFactor();
            return qFactorResult;
        }

        IEnumerable<QFactorResult> GetQFactorResults(MeasResultsList measResultsList, IEnumerable<IEnumerable<Point>> points)
        {
            QFactorSettings qFactorSettings = new();
            List<QFactorResult> result = new();
            List<QFactorResult> emptyResult = new() { new QFactorResult() };

            IEnumerable<MeasResultsList> measResultsLists = ConvertToMeasResultsLists(measResultsList, points);
            if (measResultsLists == null)
                return emptyResult;

            foreach (var newMeasResultsList in measResultsLists)
            {
                if (newMeasResultsList.Count <= 3)
                    continue;

                var qFactorCalculator = new QFactorCalculator.QFactorCalculator(newMeasResultsList, qFactorSettings);
                QFactorResult qFactorResult = qFactorCalculator.calculateQFactor();
                result.Add(qFactorResult);
            }

            return result.Count > 0 ? result : emptyResult;
        }

        IEnumerable<IEnumerable<Point>> GetLorenzeCurves(IEnumerable<IEnumerable<Point>> groupOfPoints, IEnumerable<QFactorResult> qFactorResults)
        {
            List<List<Point>> result = new();

            if (!groupOfPoints.Any() || !qFactorResults.Any())
                return result;

            int countGroupOfPoints = groupOfPoints.Count();
            int countQFactorResults = qFactorResults.Count();
            var countOfLorenze = countGroupOfPoints == countQFactorResults
                ? countGroupOfPoints
                : countQFactorResults;

            if (countOfLorenze > 10)
                return result;

            for (int i = 0; i <= (countOfLorenze - 1); i++)
            {
                IEnumerable<Point> points = groupOfPoints.ToArray()[i];
                QFactorResult qFactorResult = qFactorResults.ToArray()[i];

                List<Point> lorenzeCurve = FindMaximum.GetLorenzeCureve(points, qFactorResult).ToList();

                result.Add(lorenzeCurve);
            }

            return result;
        }

        IEnumerable<MeasResultsList> ConvertToMeasResultsLists(MeasResultsList measResultsList, IEnumerable<IEnumerable<Point>> groupOfPoints)
        {
            List<MeasResultsList> result = new();
            foreach (var group in groupOfPoints)
            {
                if (group.Count() <= 3)
                    continue;

                var measPointList = measResultsList.GetPointsInRange(group.First().X, group.Last().X ).ToList();
                MeasResultsList newMeasResultsList = new();

                foreach (var point in measPointList)
                    newMeasResultsList.addFreqPower(point.frequency, point.gain);

                result.Add(newMeasResultsList);
            }

            return result;
        }

        IEnumerable<FitCurve> GetFitCurves(IEnumerable<Point> points, IEnumerable<IEnumerable<Point>> lorenzeCurves)
        {
            List<FitCurve> result = new();

            foreach (var lorenzeCurve in lorenzeCurves)
            {
                List<Point> fitCurve = new();

                foreach (var lorenzeCurvePoint in lorenzeCurve)
                {
                    Point point = points.FirstOrDefault(x => x.X == lorenzeCurvePoint.X);
                    if (point == null)
                        continue;

                    var y = lorenzeCurvePoint.Y - point.Y;
                    Point fitCurvePoint = new() { X = point.X, Y = y };
                    fitCurve.Add(fitCurvePoint);
                }
                var isFitError = IsFitError(fitCurve);
                FitCurve fitCurveResult = new() { Points = fitCurve, IsFitError = isFitError };
                result.Add(fitCurveResult);
            }

            return result;
        }
        bool IsFitError(IEnumerable<Point> fitCurve)
        {
            double max = fitCurve.Max(x => Math.Abs(x.Y));
            return max > 1 ? true : false;
        }

    }
}
