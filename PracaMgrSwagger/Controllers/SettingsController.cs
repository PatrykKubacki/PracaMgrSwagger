using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using PracaMgrSwagger.Models;
using PracaMgrSwagger.Models.Settings;

namespace PracaMgrSwagger.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        [HttpGet("GetSettings")]
        public Settings GetSettings()
            => new Settings();

        [HttpGet("GetMainSettings")]
        public MainSettings GetMainSettings()
            => new MainSettings();

        [HttpPost("SetMainSettings")]
        public IActionResult SetMainSettings(MainSettings settings)
            => Ok();

        [HttpGet("GetConvertSettings")]
        public ConvertSettings GetConvertSettings()
            => new ConvertSettings();


        [HttpGet("GetResonators")]
        public IEnumerable<Resonator> GetResonators()
            => new List<Resonator>();

        [HttpPost("AddResonator")]
        public IActionResult AddResonator(ZipItem item, string type) 
            => Ok();

        [HttpPost("SetConvertSettings")]
        public IActionResult SetConvertSettings(int resonatorId) 
            => Ok();

        [HttpPost("SetScanSettings")]
        public IActionResult SetScanSettings(ScanSettings scanSettings)
            => Ok();
    }
}