using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    public class Config
    {
        public int Crews { get; set; }
        public string NextMowEventDate { get; set; }
        public string StartDate { get; set; }
        public List<Zone> Zones { get; set; }
    }
}
