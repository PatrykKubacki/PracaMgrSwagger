﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models.Requests
{
    public class SetHubParametersRequest
    {
        public string start { get; set; }
        public string stop { get; set; }
        public string points { get; set; }
    }
}
