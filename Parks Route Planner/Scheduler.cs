using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    internal class Scheduler
    {
        public DateTime cycleStartDate;
        public DateTime mowEventAnchor;
        public int workingDaysUsed;

        public Dictionary<int, List<string>> crewVisitHistory = new();

        public int CrewCount = 0;

        //Creates a dictionary of all possible crew pairings
        public Dictionary<string, int> crewPairs = new();

        public Dictionary<int, List<int>> previousDayPairings = new();

        //Creates a list of parks for each zone that can be manipulated during the 2 week period to determine assignments
        public Dictionary<int, List<Site>> remainingParks = new();

        public List<Zone> zones = new();

        public Scheduler(List<Zone> zoneList, int crewCount, DateTime cycleStartDate, DateTime mowEventAnchor)
        {
            this.cycleStartDate = cycleStartDate;
            this.mowEventAnchor = mowEventAnchor;
            CrewCount = crewCount;
            for (int crew = 1; crew <= CrewCount; crew++)
            {
                for (int innercrew = crew + 1; innercrew <= CrewCount; innercrew++)
                {
                    crewPairs.Add($"{crew}-{innercrew}", 0);
                }
            }
            zones = zoneList;
            foreach (Zone zone in zoneList)
            {
                remainingParks.Add(zone.ZoneId, new List<Site>(zone.Parks));
            }
            for (int crew = 1; crew <= crewCount; crew++)
            {
                crewVisitHistory.Add(crew, new List<string>());
            }
        }

        public ScheduleDay ProcessDay(string date)
        {
            List<int> chosenZones = PickZones();
            Dictionary<int, List<int>> crewsAssigned = AssignedCrews(chosenZones);
            ScheduleDay currentDay = new();
            currentDay.Date = date;
            currentDay.Assignments = new();
            int totalWorkingDays = CalendarBuilder.GetWorkingDaysInCycle(cycleStartDate, mowEventAnchor);
            int workingDaysRemaining = Math.Max(1, totalWorkingDays - workingDaysUsed);
            int totalRemainingParks = remainingParks.Values.Sum(parkList => parkList.Count);
            int parksPerCrew = Math.Max(1, totalRemainingParks / workingDaysRemaining / CrewCount);
            foreach (KeyValuePair<int, List<int>> zoneAssignment in crewsAssigned)
            {
                int crewIndex = 0;
                Zone currentZone = zones.FirstOrDefault(zone => zone.ZoneId == zoneAssignment.Key);
                foreach (int crew in zoneAssignment.Value)
                {
                    Assignment assignment = new();
                    assignment.AssignedZone = currentZone;
                    assignment.AssignedCrew = crew;
                    currentDay.Assignments.Add(assignment);
                    assignment.AssignedParks = remainingParks[zoneAssignment.Key].OrderBy(x => Random.Shared.Next()).Skip(crewIndex * parksPerCrew).Take(parksPerCrew).ToList();
                    crewIndex++;
                }
            }
            MarkParksVisited(currentDay, parksPerCrew);
            workingDaysUsed++;
            if (remainingParks.Values.All( parkList => parkList.Count == 0))
            {
                ResetCycle(DateTime.Parse(date));
            }
            return currentDay;
        }

        //Generates a list of available zones based on whether parks are left to do or not and randomly selects 2 zones and returns them
        internal List<int> PickZones()
        {
            List<int> availableZones = new();

            foreach (KeyValuePair<int, List<Site>> zone in remainingParks)
            {
                if (zone.Value.Count > 0)
                {
                    availableZones.Add(zone.Key);
                }
            }
            if (availableZones.Count == 0)
            {
                return availableZones;
            }
            List<int> selectedZones = new();
            if (availableZones.Count < 2)
            {
                return availableZones;
            }
            selectedZones = availableZones.OrderBy(x => Random.Shared.Next()).Take(2).ToList();
            return selectedZones;
        }
        internal Dictionary<int, List<int>> AssignedCrews(List<int> selectedZones)
        {
            List<int> availableCrews = new();
            for (int crew = 1; crew <= CrewCount; crew++)
            {
                availableCrews.Add(crew);
            }
            Dictionary<int, List<int>> todaysAssignments = new();
            foreach (int zoneId in selectedZones)
            {
                int counter = 0;
                List<int> chosenCrew = new();
                bool duplicateCrew = false;
                do
                {
                    chosenCrew = Random.Shared.GetItems(availableCrews.ToArray(), 2).ToList();
                    duplicateCrew = previousDayPairings.Values.Any<List<int>>(crewList => crewList.Contains(chosenCrew[0]) && crewList.Contains(chosenCrew[1]));
                    counter = counter + 1;
                    if (counter == 10)
                    {
                        break;
                    }
                }
                while (duplicateCrew);
                todaysAssignments.Add(zoneId, chosenCrew);
                availableCrews.Remove(chosenCrew[0]);
                availableCrews.Remove(chosenCrew[1]);
            }
            previousDayPairings = todaysAssignments;
            return todaysAssignments;
        }
        internal void MarkParksVisited(ScheduleDay currentDay, int parksPerCrew)
        {
            foreach (Assignment assignment in currentDay.Assignments)
            {
                int zoneId = assignment.AssignedZone.ZoneId;
                if (remainingParks[zoneId].Count >= parksPerCrew)
                {
                    var parksToVisit = remainingParks[zoneId].Take(parksPerCrew);
                    foreach (var park in parksToVisit)
                    {
                        string parkId = $"{zoneId}-{park.Park}";
                        crewVisitHistory[assignment.AssignedCrew].Add(parkId);
                    }
                    remainingParks[zoneId].RemoveRange(0, parksPerCrew);
                }
                else
                {
                    var parksToVisit = remainingParks[zoneId].Take(remainingParks[zoneId].Count);
                    foreach (var park in parksToVisit)
                    {
                        string parkId = $"{zoneId}-{park.Park}";
                        crewVisitHistory[assignment.AssignedCrew].Add(parkId);
                    }
                    remainingParks[zoneId].Clear();
                }
            }
        }

        internal void ResetCycle(DateTime newCycleStart)
        {
            cycleStartDate = newCycleStart;
            foreach (Zone zone in zones)
            {
                remainingParks[zone.ZoneId] = new List<Site>(zone.Parks);
            }
            workingDaysUsed = 0;
        }

        public bool IsGenerationComplete()
        {
            List<string> totalParkList = new();
            foreach (Zone zone in zones)
            {
                foreach (Site park in zone.Parks)
                {
                    totalParkList.Add($"{zone.ZoneId}-{park.Park}");
                }
            }
            bool crewHistoryMatchesParkList = crewVisitHistory.Values.All(crewHistory => totalParkList.All(park => crewHistory.Contains(park)));
            return crewHistoryMatchesParkList;
        }
    }
}
