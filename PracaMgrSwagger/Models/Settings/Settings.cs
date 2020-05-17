using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models.Settings
{
    public class Settings
    {
        public ConvertSettings ConvertSettings { get; set; }
        public MainSettings MainSettings { get; set; }
        public ScanSettings ScanSettings { get; set; }
        public bool EditableScanSettings { get; set; }
    }
}
