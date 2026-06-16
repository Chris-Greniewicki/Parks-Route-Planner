using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    internal class Scheduler
    {
        //Creates a dictionary of all possible crew pairings
        public Dictionary<string, int> crewPairs = new()
        {
            ["1-2"] = 0,
            ["1-3"] = 0,
            ["1-4"] = 0,
            ["2-3"] = 0,
            ["2-4"] = 0,
            ["3-4"] = 0
        };
        //Creates a list of parks for each zone that can be manipulated during the 2 week period to determine assignments
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

        //Generates a list of available zones based on whether parks are left to do or not and randomly selects 2 zones and returns them
        internal List<int> PickZones()
        {
            List<int> availableZones = new();
            foreach (KeyValuePair<int, List<Site>> zone in remainingParks)
            {
                if (zone.Value.Count > 0)
                {
                    Console.WriteLine($"Zone {zone.Key} is available");
                    availableZones.Add(zone.Key);
                }
                else
                {
                    Console.WriteLine("Nothing in Zones");
                }
            }
            List<int> selectedZones = new();
            selectedZones = Random.Shared.GetItems(availableZones.ToArray(), 2).ToList();
            return selectedZones;
        }
    }
}
