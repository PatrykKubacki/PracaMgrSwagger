using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PracaMgrSwagger.Models;

namespace PracaMgrSwagger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("Initialize")]
        public ApplicationData Initialize() 
            => new ApplicationData();

        [HttpGet("GetEmptyResonator")]
        public ResonatorParameters GetEmptyResonator() 
            => new ResonatorParameters();

        [HttpGet("GetMeasureResult")]
        public MeasureResult GetMeasureResult()
            => new MeasureResult();
    }
}