using Microsoft.AspNetCore.SignalR;
using PracaMgrSwagger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeData = PracaMgrSwagger.FakeDater.FakeData;

namespace PracaMgrSwagger.Hubs
{
    public interface IChartClient
    {
        Task SendChart(string message, ChartData chartData);
    }

    public class ChartHub: Hub<IChartClient>
    {
        public async Task SendChart()
        {
            var chart = FakeData.GetChartData();

            await Clients.All.SendChart("SendChart", chart);
        }

    }
    
}
