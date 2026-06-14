using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    public class ScheduleDay
    {
        public string Date { get; set; }
        public List<Zone> ActiveZones { get; set; }
        public List<Assignment> AssignedParks{ get; set; }
    }
}
