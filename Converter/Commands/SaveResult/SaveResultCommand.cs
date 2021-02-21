using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Converter.Commands.SaveResult
{
    public class SaveResultCommand: IRequest<Unit>
    {
        public string ResonatorType { get; set; }
        public string ResonatorName { get; set; }
    }
}
