using System;
using System.Collections.Generic;
using System.Text;

namespace Converter.Queries.GetResultFromFile.Results
{
    public class SplitPostResult: IResultFromFile
    {
        public string H { get; set; }
        public string Permittivity { get; set; }
        public string DielectricLossTangent { get; set; }
        public string Resistivity { get; set; }
        public string SheetRessistance { get; set; }
    }
}
