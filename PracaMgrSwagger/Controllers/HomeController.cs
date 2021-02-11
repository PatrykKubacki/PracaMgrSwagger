using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PracaMgrSwagger.Models;
using PracaMgrSwagger.Models.Requests;
using FakeData = PracaMgrSwagger.FakeDater.FakeData;

namespace PracaMgrSwagger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        ChartHubConnections _chartHubConnections;

        public HomeController(ChartHubConnections hubConnections)
        {
            _chartHubConnections = hubConnections;
        }

        [HttpGet("Initialize")]
        public ApplicationData Initialize() 
            => new ApplicationData();

        [HttpGet("GetEmptyResonator")]
        public ResonatorParameters GetEmptyResonator() 
            => FakeData.GetFakeEmptyResonator();

        [HttpGet("GetMeasureResult")]
        public MeasureResult GetMeasureResult()
            => new MeasureResult();

        [HttpPost("SetChartHubConnection")]
        public IActionResult SetChartHubConnection([FromBody] SetChartHubConnectionRequest request)
        {
            if (_chartHubConnections != null)
                _chartHubConnections.SetChartHubConnection(request.connectionId);

            return Ok();
        }

        [HttpPost("SetStartFrequency")]
        public IActionResult SetStartFrequency([FromBody] SetStartFrequencyRequest request)
        {
            if(_chartHubConnections != null && double.TryParse(request.value, out double value))
                _chartHubConnections.SetStartFrequency(request.connectionId, value);

            return Ok();
        }

        [HttpPost("SetStopFrequency")]
        public IActionResult SetStopFrequency([FromBody] SetStopFrequencyRequest request)
        {
            if (_chartHubConnections != null && double.TryParse(request.value, out double value))
                _chartHubConnections.SetStopFrequency(request.connectionId, value);

            return Ok();
        }

    }
}