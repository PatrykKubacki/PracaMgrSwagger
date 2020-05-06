using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class Resonator
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ResonatorType Type { get; set; }
    }

    public enum ResonatorType
    {
        SinglePost = 1,
        SplitPost = 2,
    }

}
