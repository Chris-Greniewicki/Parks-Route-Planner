using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    internal class Scheduler
    {
        public List<int> pendingZones = new();
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
            foreach (Zone zone in zoneList)
            {
                pendingZones.Add(zone.ZoneId);
            }
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
            List<int> largeZones = chosenZones.Where(zone => remainingParks[zone].Any(park => park.isLarge)).ToList();
            Dictionary<int, List<int>> crewsAssigned = AssignedCrews(chosenZones, largeZones);
            ScheduleDay currentDay = new();
            currentDay.Date = date;
            currentDay.Assignments = new();
            int totalWorkingDays = CalendarBuilder.GetWorkingDaysInCycle(cycleStartDate, mowEventAnchor);
            int workingDaysRemaining = Math.Max(1, totalWorkingDays - workingDaysUsed);
            int totalRemainingParks = remainingParks.Values.Sum(parkList => parkList.Count);
            foreach (KeyValuePair<int, List<int>> zoneAssignment in crewsAssigned)
            {
                if (largeZones.Contains(zoneAssignment.Key))
                {
                    var shuffledParks = remainingParks[zoneAssignment.Key].OrderBy(x => Random.Shared.Next()).ToList();
                    Site largePark = shuffledParks.First(p => p.isLarge);
                    List<Site> regularParks = shuffledParks.Where(p => !p.isLarge).ToList();

                    List<Site> crew1Parks = new List<Site> { largePark, regularParks[0] };
                    List<Site> crew2Parks = new List<Site> { largePark, regularParks.Count > 1 ? regularParks[1] : regularParks[0] };

                    Assignment assignment1 = new();
                    assignment1.AssignedZone = zones.FirstOrDefault(z => z.ZoneId == zoneAssignment.Key);
                    assignment1.AssignedCrew = zoneAssignment.Value[0];
                    assignment1.AssignedParks = crew1Parks;
                    currentDay.Assignments.Add(assignment1);
                    foreach (var park in crew1Parks)
                        crewVisitHistory[assignment1.AssignedCrew].Add($"{zoneAssignment.Key}-{park.Park}");

                    Assignment assignment2 = new();
                    assignment2.AssignedZone = zones.FirstOrDefault(z => z.ZoneId == zoneAssignment.Key);
                    assignment2.AssignedCrew = zoneAssignment.Value[1];
                    assignment2.AssignedParks = crew2Parks;
                    currentDay.Assignments.Add(assignment2);
                    foreach (var park in crew2Parks)
                        crewVisitHistory[assignment2.AssignedCrew].Add($"{zoneAssignment.Key}-{park.Park}");

                    remainingParks[zoneAssignment.Key] = shuffledParks
                        .Where(p => p != largePark && p != regularParks[0] && (regularParks.Count <= 1 || p != regularParks[1]))
                        .ToList();
                }
                else
                {
                    int zoneParksPerCrew = Math.Max(1, Math.Min(2, remainingParks[zoneAssignment.Key].Count / 2));
                    var shuffledParks = remainingParks[zoneAssignment.Key].OrderBy(x => Random.Shared.Next()).ToList();
                    List<Site> crew1Parks = shuffledParks.Take(zoneParksPerCrew).ToList();
                    List<Site> crew2Parks = shuffledParks.Skip(zoneParksPerCrew).Take(zoneParksPerCrew).ToList();

                    Assignment assignment1 = new();
                    assignment1.AssignedZone = zones.FirstOrDefault(z => z.ZoneId == zoneAssignment.Key);
                    assignment1.AssignedCrew = zoneAssignment.Value[0];
                    assignment1.AssignedParks = crew1Parks;
                    currentDay.Assignments.Add(assignment1);
                    foreach (var park in crew1Parks)
                        crewVisitHistory[assignment1.AssignedCrew].Add($"{zoneAssignment.Key}-{park.Park}");

                    Assignment assignment2 = new();
                    assignment2.AssignedZone = zones.FirstOrDefault(z => z.ZoneId == zoneAssignment.Key);
                    assignment2.AssignedCrew = zoneAssignment.Value[1];
                    assignment2.AssignedParks = crew2Parks;
                    currentDay.Assignments.Add(assignment2);
                    foreach (var park in crew2Parks)
                        crewVisitHistory[assignment2.AssignedCrew].Add($"{zoneAssignment.Key}-{park.Park}");

                    remainingParks[zoneAssignment.Key] = shuffledParks.Skip(zoneParksPerCrew * 2).ToList();
                }
            }
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
            List<int> pendingWithParks = pendingZones.Where(z => remainingParks[z].Count > 0).ToList();
            if (pendingWithParks.Count >= 2)
            {
                selectedZones = pendingWithParks.OrderBy(x => Random.Shared.Next()).Take(2).ToList();
            }
            else if (pendingWithParks.Count == 1)
            {
                selectedZones.Add(pendingWithParks[0]);
                List<int> others = availableZones.Where(z => !selectedZones.Contains(z)).OrderBy(x => Random.Shared.Next()).ToList();
                if (others.Count > 0) selectedZones.Add(others[0]);
            }
            else
            {
                selectedZones = availableZones.OrderBy(x => Random.Shared.Next()).Take(2).ToList();
            }
            foreach (int zoneId in selectedZones)
            {
                pendingZones.Remove(zoneId);
            }
            return selectedZones;
        }
        internal Dictionary<int, List<int>> AssignedCrews(List<int> selectedZones, List<int> largeZones)
        {
            List<int> availableCrews = new();
            for (int crew = 1; crew <= CrewCount; crew++)
            {
                availableCrews.Add(crew);
            }
            Dictionary<int, List<int>> todaysAssignments = new();
            foreach (int zoneId in selectedZones)
            {
                List<int> chosenCrew = new();
                if (largeZones.Contains(zoneId))
                {
                    // Find the pair with lowest crewPairs count from available crews
                    string bestPairKey = crewPairs
                        .Where(p => {
                            var parts = p.Key.Split('-');
                            int a = int.Parse(parts[0]);
                            int b = int.Parse(parts[1]);
                            return availableCrews.Contains(a) && availableCrews.Contains(b);
                        })
                        .OrderBy(p => p.Value)
                        .First().Key;
                    var pairParts = bestPairKey.Split('-');
                    chosenCrew = new List<int> { int.Parse(pairParts[0]), int.Parse(pairParts[1]) };
                    crewPairs[bestPairKey]++;
                }
                else
                {
                    int counter = 0;
                    bool duplicateCrew = false;
                    do
                    {
                        chosenCrew = availableCrews.OrderBy(x => Random.Shared.Next()).Take(2).ToList();
                        duplicateCrew = previousDayPairings.Values.Any<List<int>>(crewList => crewList.Contains(chosenCrew[0]) && crewList.Contains(chosenCrew[1]));
                        counter++;
                        if (counter == 10) break;
                    }
                    while (duplicateCrew);
                }
                todaysAssignments.Add(zoneId, chosenCrew);
                availableCrews.Remove(chosenCrew[0]);
                availableCrews.Remove(chosenCrew[1]);
            }
            previousDayPairings = todaysAssignments;
            return todaysAssignments;
        }

        internal void ResetCycle(DateTime newCycleStart)
        {
            cycleStartDate = newCycleStart;
            foreach (Zone zone in zones)
            {
                remainingParks[zone.ZoneId] = new List<Site>(zone.Parks);
            }
            workingDaysUsed = 0;
            pendingZones.Clear();
            foreach (Zone zone in zones)
            {
                pendingZones.Add(zone.ZoneId);
            }
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
