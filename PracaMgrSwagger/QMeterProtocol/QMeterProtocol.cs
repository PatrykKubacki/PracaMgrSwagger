﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PracaMgrSwagger.Helpers;
using PracaMgrSwagger.Hubs;
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
            ChartData = new ChartData();
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
                Thread.Sleep(300);
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
                            if (_chartHubConnections != null &&
                                _chartHubConnections.Connection != null &&
                                (_minFrequency != _chartHubConnections.Connection.StartFrequency || _maxFrequency != _chartHubConnections.Connection.StopFrequency))
                            {
                                _minFrequency = _chartHubConnections.Connection.StartFrequency;
                                _maxFrequency = _chartHubConnections.Connection.StopFrequency;
                                var start = Convert.ToInt32(_minFrequency) * 1000;
                                var stop = Convert.ToInt32(_maxFrequency) * 1000;
                                start = start != 0 ? start : _ph.QMeterMinFrequency * 1000;
                                stop = stop != 0 ? stop : _ph.QMeterMaxFrequency * 1000;
                                _ph.cmdSetScanRange(start, stop, 8000, ProtocolHandler.MeasType.mtS21, _oversampling, 0);
                            }
                        }
                    } 

                    _mrl = _measManager.getActualMeasResultsList();
                    if (_mrl != null)
                    {
                        var measuredPointsPerSecond = _measManager.LastMeasRate;
                        ChartData = ConvertDeviceDataToResultChartData(_mrl, measuredPointsPerSecond);
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
            

        ChartData ConvertDeviceDataToResultChartData(MeasResultsList measResultsList, int measuredPointsPerSecond)
        {
            ChartData result = new();
            var pointList = measResultsList.getPointList();

            result.Points = pointList.Select(point => new Point { X = point.frequency, Y = point.gain })
                                     .ToList();

            result.Maximums = MaximumHelper.GetMaximumGroups(result.Points);
            result.GroupsOfPoints = MaximumHelper.GetGroupOfMaximumsPoints(result.Points, result.Maximums);
            result.QFactorResults = GetQFactorResults(measResultsList, result.GroupsOfPoints);
            result.LorenzeCurves = LorenzeCurveHelper.GetLorenzeCurves(result.GroupsOfPoints, result.QFactorResults);
            result.FitCurves = FitErrorCurveHelper.GetFitCurves(result.Points, result.LorenzeCurves);
            result.MeasuredPointsPerSecond = measuredPointsPerSecond;

            return result;
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

                if(qFactorResult.Q_factor != 0 && qFactorResult.Bandwidth != 0 && qFactorResult.NumberOfPoints != 0)
                    result.Add(qFactorResult);
            }

            return result.Count > 0 ? result : emptyResult;
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
    }
}
