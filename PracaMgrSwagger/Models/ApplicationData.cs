using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AllSettings = PracaMgrSwagger.Models.Settings.Settings;

namespace PracaMgrSwagger.Models
{
    public class ApplicationData
    {
        public ResonatorParameters ResonatorParameters { get; set; }
        public AllSettings Settings { get; set; }
    }
}
