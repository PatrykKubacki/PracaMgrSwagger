﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models.Requests
{
    public class SetPointsOnScreenRequest
    {
        public string connectionId { get; set; }
        public string value { get; set; }
    }
}
