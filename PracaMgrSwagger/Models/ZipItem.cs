using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PracaMgrSwagger.Models
{
    public class ZipItem
    { 
        public string Name { get; set; }
        public Stream Content { get; set; }

        public ZipItem(string name, Stream content)
        {
            this.Name = name;
            this.Content = content;
        }

    }
}
