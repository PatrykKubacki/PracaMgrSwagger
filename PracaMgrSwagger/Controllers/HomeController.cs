using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Converter.Commands.SaveData;
using Converter.Commands.SaveResult;
using Converter.Queries.GetResultFromFile;
using Converter.Queries.GetResultFromFile.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PracaMgrSwagger.Models;
using PracaMgrSwagger.Models.Requests;
using FakeDatas = PracaMgrSwagger.FakeDater.FakeData;

namespace PracaMgrSwagger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        IMediator _mediator;
        ChartHubConnections _chartHubConnections;
        
        public HomeController(ChartHubConnections hubConnections, IMediator mediator)
        {
            _chartHubConnections = hubConnections;
            _mediator = mediator;
        }

        [HttpGet("Initialize")]
        public ApplicationData Initialize() 
            => new ApplicationData();

        [HttpGet("GetEmptyResonator")]
        public ResonatorParameters GetEmptyResonator() 
            => FakeDatas.GetFakeEmptyResonator();

        [HttpGet("GetMeasureResult")]
        public MeasureResult GetMeasureResult()
            => new MeasureResult();

        [HttpPost("UnZoomFull")]
        public IActionResult UnZoomFull([FromBody] UnZoomFullRequest request)
        {
            if (_chartHubConnections != null)
                _chartHubConnections.UnZoomFull(request.connectionId);

            return Ok();
        }

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

        [HttpPost("SetPointsOnScreen")]
        public IActionResult SetPointsOnScreen([FromBody] SetPointsOnScreenRequest request)
        {
            if (_chartHubConnections != null && int.TryParse(request.value, out int value))
                _chartHubConnections.SetPointsOnScreen(request.connectionId, value);

            return Ok();
        }

        [HttpPost("SetStartStopRangeFrequency")]
        public IActionResult SetStartStopRangeFrequency([FromBody] SetStartStopRangeFrequencyRequest request)
        {
            if (_chartHubConnections != null && double.TryParse(request.start, out double start) && double.TryParse(request.stop, out double stop))
                _chartHubConnections.SetStartStopFrequency(request.connectionId, start, stop);

            return Ok();
        }


        [HttpPost("GetConverterResult")]
        public async Task<IResultFromFile> GetConverterResult([FromBody] GetConverterResultRequest request)
        {
            var saveDataCommand = new SaveDataCommand
            {
                CenterFrequency = request.CenterFrequency,
                H = request.H,
                QFactor = request.QFactor,
                ResonatorName = request.ResonatorName,
                ResonatorType = request.ResonatorType,
                UnloadedCenterFrequency = request.UnloadedCenterFrequency,
                UnloadedQ = request.UnloadedQ,
            };
            await _mediator.Send(saveDataCommand);

            var saveResultCommand = new SaveResultCommand
            {
                ResonatorName = request.ResonatorName,
                ResonatorType = request.ResonatorType,
            };
            await _mediator.Send(saveResultCommand);

            var getResultFromFileQuery = new GetResultFromFileQuery
            {
                ResonatorName = request.ResonatorName,
                ResonatorType = request.ResonatorType,
            };
            var result = await _mediator.Send(getResultFromFileQuery);

            return result;
        }

    }
}