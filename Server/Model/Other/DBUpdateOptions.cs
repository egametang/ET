using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class DBUpdateOptions
    {
        public string Filter { get; set; }

        public string Update { get; set; }

        public bool IsUpsert { get; set; }
    }
}
