using Converter.Queries.GetResultFromFile.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Converter.Queries.GetResultFromFile
{
    public class GetResultFromFileQuery: IRequest<IResultFromFile>
    {
        public string ResonatorType { get; set; }
        public string ResonatorName { get; set; }
    }
}
