using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    public class ScheduleDay
    {
        public string Date { get; set; }
        public List<Assignment> Assignments { get; set; }
        public List<int> SupplementalCrews { get; set; } = new();
    }
}
