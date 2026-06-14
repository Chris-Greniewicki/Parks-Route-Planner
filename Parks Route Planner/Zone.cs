using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    public class Zone
    {
        public int ZoneId { get; set; }
        public List<Site> Parks { get; set; }
    }
}
