using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class DBFindOptions
    {
        public string Filter { get; set; }

        public string Projection { get; set; }

        public string Sort { get; set; }

        public int? Limit { get; set; }

        public int? Skip { get; set; }
    }
}
