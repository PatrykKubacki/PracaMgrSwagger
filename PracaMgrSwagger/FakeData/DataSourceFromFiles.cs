using PracaMgrSwagger.Models;
using QFactorCalculator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.FakeData
{
    public class DataSourceFromFiles
    {
        public string[] FilesNames = { "spdr 5 q-factor.s2p", "spdr 5g close1.s2p", "spdr 5g close2.s2p", "spdr 5g wide.s2p", "spdr 5g wide1.s2p", "spdr 6g close.s2p" };
        public Dictionary<string, DataSetFromFile> DataSets { get; set; }

        public DataSourceFromFiles()
        {
            ReadAllDataFromFile();
        }

        void ReadAllDataFromFile()
        {
            DataSets = new Dictionary<string, DataSetFromFile>();
            foreach (var fileName in FilesNames)
            {
                var measResultsList = new MeasResultsList();
                var path = Path.Combine(Environment.CurrentDirectory, @"Assets\", fileName);
                measResultsList.readFromS2P(path);
                DataSets.Add(fileName, new DataSetFromFile(measResultsList));
            }
        }

        public MeasResultsList GetMeasResultsList(ChartHubParameters hubParameters)
        {
            if (hubParameters == null)
                return GetFullRangeMeasResultsList();

            if (hubParameters.StartFrequency == 0 || hubParameters.StopFrequency == 0)
                return GetFilteredFullRangeMeasResultsList(hubParameters);

            var keys = new List<string> { "spdr 5 q-factor.s2p", "spdr 5g close2.s2p", "spdr 5g close1.s2p", "spdr 6g close.s2p", "spdr 5g wide1.s2p" };
            MeasResultsList result = GetMeasResultsListInRange(keys, hubParameters);

            return result ?? GetFilteredFullRangeMeasResultsList(hubParameters);

        }

        MeasResultsList GetFullRangeMeasResultsList()
            => GetMeasResultsListFromFile(Get5gWideFilePath());

        MeasResultsList GetFilteredFullRangeMeasResultsList(ChartHubParameters hubParameters)
            => GetFilteredResultList(Get5gWideFilePath(), hubParameters);

        string Get5gWideFilePath()
            => Path.Combine(Environment.CurrentDirectory, @"Assets\spdr 5g wide.s2p");

        MeasResultsList GetMeasResultsListInRange(List<string> keys, ChartHubParameters hubParameters)
        {
            foreach (var key in keys)
            {
                var dataSet = DataSets[key];
                if (hubParameters.StartFrequency >= (dataSet.Start /1_000_000) && hubParameters.StopFrequency <= (dataSet.Stop / 1_000_000))
                {
                    var path = Path.Combine(Environment.CurrentDirectory, @"Assets\", key);
                    MeasResultsList measResultsList = GetFilteredResultList(path, hubParameters);
                    return measResultsList;
                }
            }
            return null;
        }

        MeasResultsList GetMeasResultsListFromFile(string path)
        {
            MeasResultsList measResultsList = new();
            measResultsList.readFromS2P(path);
            return measResultsList;
        }

        MeasResultsList GetFilteredResultList(string path, ChartHubParameters hubParameters)
        {
            MeasResultsList measResultsList = GetMeasResultsListFromFile(path);
            measResultsList.FilterPointList(hubParameters.StartFrequency * 1_000_000, hubParameters.StopFrequency * 1_000_000);
            measResultsList.CutPoints(hubParameters.PointsOnScreen);
            return measResultsList;
        }

    }

    public class DataSetFromFile
    {
        public DataSetFromFile(MeasResultsList measResultsList)
        {
            Points = measResultsList;
        }

        public double? Start => Points.getPointList().FirstOrDefault().frequency;
        public double? Stop => Points.getPointList().LastOrDefault().frequency;
        public MeasResultsList Points { get; set; }
    }
}
