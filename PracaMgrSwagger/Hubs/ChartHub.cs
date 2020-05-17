using Microsoft.AspNetCore.SignalR;
using PracaMgrSwagger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Hubs
{
    public interface IChartClient
    {
        Task Send(string message, ChartData chartData);
    }

    public class ChartHub: Hub<IChartClient>
    {
        public async Task SendMessage()
        {
            var xxx = new ChartData
            {
                Points = new List<Point>()
            };

            await Clients.All.Send("ReceiveMessage", xxx);
        }
    }
    
}
