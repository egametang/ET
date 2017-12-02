using System;

namespace Model
{
    public class DBFindAndDeleteOptions
    {
        public TimeSpan? MaxTime { get; set; }

        public string Filter { get; set; }

        public string Projection { get; set; }

        public string Sort { get; set; }
    }
}
