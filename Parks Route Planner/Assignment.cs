using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    public class Assignment
    {
        public int AssignedCrew { get; set; }
        public Zone AssignedZone { get; set; }
        public List<Site> AssignedParks { get; set; }
    }
}
