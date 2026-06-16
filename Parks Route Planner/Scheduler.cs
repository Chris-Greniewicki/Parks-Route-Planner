using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    internal class Scheduler
    {
        public Dictionary<string, int> crewPairs = new()
        {
            ["1-2"] = 0,
            ["1-3"] = 0,
            ["1-4"] = 0,
            ["2-3"] = 0,
            ["2-4"] = 0,
            ["3-4"] = 0
        };
        public Dictionary<int, List<Site>> remainingParks = new();

        public List<Zone> zones = new();

        public Scheduler(List<Zone> zoneList)
        {
            zones = zoneList;
            foreach (Zone zone in zoneList)
            {
                remainingParks.Add(zone.ZoneId, new List<Site>(zone.Parks));
            }
        }
    }
}
