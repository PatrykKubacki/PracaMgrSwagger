﻿using System;
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
using PracaMgrSwagger.Commands;
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


        [HttpPost("UnZoomFull")]
        public IActionResult UnZoomFull([FromBody] UnZoomFullRequest request)
        {
            if (_chartHubConnections != null)
                _chartHubConnections.UnZoomFull();

            return Ok();
        }

        [HttpPost("SetStartFrequency")]
        public IActionResult SetStartFrequency([FromBody] SetStartFrequencyRequest request)
        {
            if(_chartHubConnections != null && double.TryParse(request.value.Replace('.', ','), out double value))
                _chartHubConnections.SetStartFrequency(value);

            return Ok();
        }

        [HttpPost("SetStopFrequency")]
        public IActionResult SetStopFrequency([FromBody] SetStopFrequencyRequest request)
        {
            if (_chartHubConnections != null && double.TryParse(request.value.Replace('.', ','), out double value))
                _chartHubConnections.SetStopFrequency(value);

            return Ok();
        }

        [HttpPost("SetPointsOnScreen")]
        public IActionResult SetPointsOnScreen([FromBody] SetPointsOnScreenRequest request)
        {
            if (_chartHubConnections != null && int.TryParse(request.value.Replace('.', ','), out int value))
                _chartHubConnections.SetPointsOnScreen(value);

            return Ok();
        }

        [HttpPost("SetStartStopRangeFrequency")]
        public IActionResult SetStartStopRangeFrequency([FromBody] SetStartStopRangeFrequencyRequest request)
        {
            if (_chartHubConnections != null &&
                double.TryParse(request.start.Replace('.', ','), out double start) &&
                double.TryParse(request.stop.Replace('.', ','), out double stop))
                    _chartHubConnections.SetStartStopFrequency(start, stop);

            return Ok();
        }

        [HttpPost("SetHubParameters")]
        public IActionResult SetHubParameters([FromBody] SetHubParametersRequest request)
        {
            if (_chartHubConnections != null &&
                double.TryParse(request.start.Replace('.', ','), out double start) &&
                double.TryParse(request.stop.Replace('.', ','), out double stop) &&
                int.TryParse(request.points, out int points)) 
                _chartHubConnections.SetHubParameters(start, stop, points);

            return Ok();
        }


        [HttpPost("PutOnOfObject")]
        public IActionResult PutOnOfObject([FromBody] PutOnOfObjectRequest request)
        {
            if (_chartHubConnections != null)
                _chartHubConnections.SetIsObjectInside(request.IsObjectInside);

            return Ok();
        }

        [HttpPost("SaveMeasurements")]
        public async Task<IEnumerable<string>> SaveMeasurements([FromBody] SaveMeasurmentsRequest request)
        {
            var saveMeasurementsCommand = new SaveMeasurementsCommand
            {
                SessionName = request.SessionName,
                Measurements = request.Measurements
            };

            await _mediator.Send(saveMeasurementsCommand);

            var getSavedMeasurementsFilesList = new GetSavedMeasurementsFilesListCommand();
            var savedMeasurementsFilesList = await _mediator.Send(getSavedMeasurementsFilesList);

            return savedMeasurementsFilesList;
        }

        [HttpGet("GetSavedMeasurementsFilesList")]
        public async Task<IEnumerable<string>> GetSavedMeasurementsFilesList()
        {
            var getSavedMeasurementsFilesList = new GetSavedMeasurementsFilesListCommand();
            var savedMeasurementsFilesList = await _mediator.Send(getSavedMeasurementsFilesList);

            return savedMeasurementsFilesList;
        }

        [HttpPost("GetSavedMeasurementsSession")]
        public async Task<IEnumerable<MeasureResult>> GetSavedMeasurementsSession([FromBody] GetSavedMeasurmentsSessionRequest request)
        {
            GetSavedMeasurementsSessionCommand getSavedMeasurementsSessionCommand = new () { SessionName = request.SessionName };
            var savedSession = await _mediator.Send(getSavedMeasurementsSessionCommand);

            return savedSession;
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