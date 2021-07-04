using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Commands
{
    public class GetSavedMeasurementsFilesListCommand: IRequest<List<string>>
    {
    }
}
