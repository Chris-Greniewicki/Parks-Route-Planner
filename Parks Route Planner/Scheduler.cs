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
        public Dictionary<int, int> crewSitOutHistory = new();
        public int supplementalCrewIndex = 0;

        public int CrewCount = 0;

        // Creates a dictionary of all possible crew pairings
        public Dictionary<string, int> crewPairs = new();

        public Dictionary<int, List<int>> previousDayPairings = new();

        // Creates a list of parks for each zone that can be manipulated during the 2 week period
        public Dictionary<int, List<Site>> remainingParks = new();

        public List<Zone> zones = new();

        // Receives data to construct schedule
        public Scheduler(List<Zone> zoneList, int crewCount, DateTime cycleStartDate, DateTime mowEventAnchor)
        {
            foreach (Zone zone in zoneList)
                pendingZones.Add(zone.ZoneId);

            this.cycleStartDate = cycleStartDate;
            this.mowEventAnchor = mowEventAnchor;
            CrewCount = crewCount;

            for (int crew = 1; crew <= CrewCount; crew++)
            {
                for (int innercrew = crew + 1; innercrew <= CrewCount; innercrew++)
                    crewPairs.Add($"{crew}-{innercrew}", 0);
            }

            zones = zoneList;

            foreach (Zone zone in zoneList)
                remainingParks.Add(zone.ZoneId, new List<Site>(zone.Parks));

            for (int crew = 1; crew <= crewCount; crew++)
            {
                crewVisitHistory.Add(crew, new List<string>());
                crewSitOutHistory.Add(crew, 0);
            }
        }

        // Main public method to handle all actions to generate a schedule for a day
        public ScheduleDay ProcessDay(string date)
        {
            List<int> chosenZones = PickZones();
            List<int> largeZones = chosenZones.Where(zone => remainingParks[zone].Any(park => park.isLarge)).ToList();
            Dictionary<int, List<int>> crewsAssigned = AssignedCrews(chosenZones, largeZones, out List<int> supplementalCrews);

            ScheduleDay currentDay = new();
            currentDay.Date = date;
            currentDay.Assignments = new();
            currentDay.SupplementalCrews = supplementalCrews;

            int totalWorkingDays = CalendarBuilder.GetWorkingDaysInCycle(cycleStartDate, mowEventAnchor);

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
                        crewVisitHistory[assignment1.AssignedCrew].Add($"{zoneAssignment.Key}-{park.Park.Trim()}");

                    Assignment assignment2 = new();
                    assignment2.AssignedZone = zones.FirstOrDefault(z => z.ZoneId == zoneAssignment.Key);
                    assignment2.AssignedCrew = zoneAssignment.Value[1];
                    assignment2.AssignedParks = crew2Parks;
                    currentDay.Assignments.Add(assignment2);
                    foreach (var park in crew2Parks)
                        crewVisitHistory[assignment2.AssignedCrew].Add($"{zoneAssignment.Key}-{park.Park.Trim()}");

                    remainingParks[zoneAssignment.Key] = shuffledParks
                        .Where(p => p != largePark && p != regularParks[0] && (regularParks.Count <= 1 || p != regularParks[1]))
                        .ToList();
                }
                else
                {
                    int crew1Id = zoneAssignment.Value[0];
                    int crew2Id = zoneAssignment.Value.Count > 1 ? zoneAssignment.Value[1] : -1;

                    int parksAvailable = remainingParks[zoneAssignment.Key].Count;
                    int daysRemaining = Math.Max(1, totalWorkingDays - workingDaysUsed);
                    int estimatedZoneDays = Math.Max(1, (daysRemaining * 2) / zones.Count);
                    int neededPerCrew = (int)Math.Ceiling(parksAvailable / 2.0 / estimatedZoneDays);
                    int perCrew = Math.Max(1, Math.Min(3, neededPerCrew));
                    if (parksAvailable >= 4)
                        perCrew = Math.Max(2, perCrew);

                    var parksForCrew1 = remainingParks[zoneAssignment.Key]
                        .OrderByDescending(p => !crewVisitHistory[crew1Id].Contains($"{zoneAssignment.Key}-{p.Park.Trim()}"))
                        .ThenBy(x => Random.Shared.Next())
                        .ToList();

                    List<Site> crew1Parks = parksForCrew1.Take(perCrew).ToList();
                    HashSet<Site> assigned = new HashSet<Site>(crew1Parks);

                    List<Site> crew2Parks = new List<Site>();
                    if (crew2Id > 0)
                    {
                        var parksForCrew2 = remainingParks[zoneAssignment.Key]
                            .Where(p => !assigned.Contains(p))
                            .OrderByDescending(p => !crewVisitHistory[crew2Id].Contains($"{zoneAssignment.Key}-{p.Park.Trim()}"))
                            .ThenBy(x => Random.Shared.Next())
                            .ToList();

                        crew2Parks = parksForCrew2.Take(perCrew).ToList();
                        assigned.UnionWith(crew2Parks);
                    }

                    remainingParks[zoneAssignment.Key] = remainingParks[zoneAssignment.Key]
                        .Where(p => !assigned.Contains(p))
                        .ToList();

                    Assignment assignment1 = new();
                    assignment1.AssignedZone = zones.FirstOrDefault(z => z.ZoneId == zoneAssignment.Key);
                    assignment1.AssignedCrew = crew1Id;
                    assignment1.AssignedParks = crew1Parks;
                    currentDay.Assignments.Add(assignment1);
                    foreach (var park in crew1Parks)
                        crewVisitHistory[crew1Id].Add($"{zoneAssignment.Key}-{park.Park.Trim()}");

                    if (crew2Id > 0)
                    {
                        Assignment assignment2 = new();
                        assignment2.AssignedZone = zones.FirstOrDefault(z => z.ZoneId == zoneAssignment.Key);
                        assignment2.AssignedCrew = crew2Id;
                        assignment2.AssignedParks = crew2Parks;
                        currentDay.Assignments.Add(assignment2);
                        foreach (var park in crew2Parks)
                            crewVisitHistory[crew2Id].Add($"{zoneAssignment.Key}-{park.Park.Trim()}");
                    }
                }
            }

            workingDaysUsed++;
            return currentDay;
        }

        // Generates a list of available zones and selects the appropriate number based on crew count
        internal List<int> PickZones()
        {
            List<int> availableZones = new();
            List<int> soloZones = new();

            foreach (KeyValuePair<int, List<Site>> zone in remainingParks)
            {
                if (zone.Value.Count >= 2)
                    availableZones.Add(zone.Key);
                else if (zone.Value.Count == 1)
                    soloZones.Add(zone.Key);
            }

            if (availableZones.Count < 2)
                availableZones.AddRange(soloZones.Where(z => !availableZones.Contains(z)));

            if (availableZones.Count == 0)
                return availableZones;

            // How many zones to work today based on crew count
            int zonesNeeded = Math.Max(1, CrewCount / 2);

            List<int> selectedZones = new();

            if (availableZones.Count == 1)
            {
                selectedZones.Add(availableZones[0]);
                pendingZones.Remove(availableZones[0]);
                return selectedZones;
            }

            List<int> pendingWithParks = pendingZones
                .Where(z => availableZones.Contains(z) && remainingParks[z].Count >= 2)
                .ToList();

            if (pendingWithParks.Count == 0)
            {
                pendingZones.AddRange(remainingParks
                    .Where(z => z.Value.Count > 0 && !pendingZones.Contains(z.Key))
                    .Select(z => z.Key));
                pendingWithParks = pendingZones
                    .Where(z => availableZones.Contains(z) && remainingParks[z].Count >= 2)
                    .ToList();
            }

            // Score each candidate zone by total unseen crew-park gaps it can resolve
            Func<int, int> zoneGapScore = zoneId =>
            {
                var zoneParkList = zones.First(z => z.ZoneId == zoneId).Parks;
                int totalPossible = CrewCount * zoneParkList.Count;
                if (totalPossible == 0) return 0;
                int gaps = crewVisitHistory.Keys.Sum(crew =>
                    zoneParkList.Count(park => !crewVisitHistory[crew].Contains($"{zoneId}-{park.Park.Trim()}")));
                return (gaps * 100) / totalPossible;
            };

            if (pendingWithParks.Count >= zonesNeeded)
                selectedZones = pendingWithParks
                    .OrderByDescending(zoneGapScore)
                    .ThenBy(x => Random.Shared.Next())
                    .Take(zonesNeeded)
                    .ToList();
            else if (pendingWithParks.Count >= 1)
            {
                selectedZones.AddRange(pendingWithParks
                    .OrderByDescending(zoneGapScore)
                    .ThenBy(x => Random.Shared.Next()));
                List<int> others = availableZones
                    .Where(z => !selectedZones.Contains(z))
                    .OrderByDescending(zoneGapScore)
                    .ThenBy(x => Random.Shared.Next())
                    .ToList();
                foreach (int z in others)
                {
                    if (selectedZones.Count >= zonesNeeded) break;
                    selectedZones.Add(z);
                }
            }
            else
                selectedZones = availableZones
                    .OrderByDescending(zoneGapScore)
                    .ThenBy(x => Random.Shared.Next())
                    .Take(zonesNeeded)
                    .ToList();

            // Keep adding zones until we have enough parks for all assignable crews
            int assignableCrews = (CrewCount / 2) * 2;
            int totalSelectedParks = selectedZones.Sum(z => remainingParks[z].Count);
            while (totalSelectedParks < assignableCrews * 2)
            {
                List<int> remaining = remainingParks
                    .Where(z => z.Value.Count > 0 && !selectedZones.Contains(z.Key))
                    .OrderByDescending(z => z.Value.Count)
                    .Select(z => z.Key)
                    .ToList();

                if (remaining.Count == 0) break;

                selectedZones.Add(remaining[0]);
                totalSelectedParks += remainingParks[remaining[0]].Count;
            }

            foreach (int zoneId in selectedZones)
                pendingZones.Remove(zoneId);

            return selectedZones;
        }

        // Assigns crews to zones with strict round-robin supplemental rotation
        internal Dictionary<int, List<int>> AssignedCrews(List<int> selectedZones, List<int> largeZones, out List<int> supplementalCrews)
        {
            supplementalCrews = new List<int>();

            List<int> allCrews = Enumerable.Range(1, CrewCount).ToList();

            if (CrewCount % 2 != 0)
            {
                // Strict round-robin: crews sit out in order 1,2,3,4,5,1,2,3,4,5,...
                // supplementalCrewIndex tracks whose turn it is globally across all days
                int crewToSitOut = allCrews[supplementalCrewIndex % allCrews.Count];
                supplementalCrewIndex++;

                supplementalCrews.Add(crewToSitOut);
                crewSitOutHistory[crewToSitOut]++;
                allCrews.Remove(crewToSitOut);
            }

            List<int> availableCrews = allCrews;
            Dictionary<int, List<int>> todaysAssignments = new();

            foreach (int zoneId in selectedZones)
            {
                if (availableCrews.Count < 2) break;
                List<int> chosenCrew = new();

                if (largeZones.Contains(zoneId))
                {
                    var validPairs = crewPairs
                        .Where(p => {
                            var parts = p.Key.Split('-');
                            int a = int.Parse(parts[0]);
                            int b = int.Parse(parts[1]);
                            return availableCrews.Contains(a) && availableCrews.Contains(b);
                        })
                        .OrderBy(p => p.Value)
                        .ThenByDescending(p => {
                            var parts = p.Key.Split('-');
                            int a = int.Parse(parts[0]);
                            int b = int.Parse(parts[1]);
                            return remainingParks[zoneId]
                                .Count(park => !crewVisitHistory[a].Contains($"{zoneId}-{park.Park.Trim()}"))
                                + remainingParks[zoneId]
                                .Count(park => !crewVisitHistory[b].Contains($"{zoneId}-{park.Park.Trim()}"));
                        })
                        .ToList();

                    if (validPairs.Count == 0)
                        chosenCrew = availableCrews.OrderBy(x => Random.Shared.Next()).Take(2).ToList();
                    else
                    {
                        string bestPairKey = validPairs.First().Key;
                        var pairParts = bestPairKey.Split('-');
                        chosenCrew = new List<int> { int.Parse(pairParts[0]), int.Parse(pairParts[1]) };
                        crewPairs[bestPairKey]++;
                    }
                }
                else
                {
                    chosenCrew = availableCrews
                        .OrderByDescending(crew => remainingParks[zoneId]
                            .Count(park => !crewVisitHistory[crew].Contains($"{zoneId}-{park.Park.Trim()}")))
                        .ThenBy(x => Random.Shared.Next())
                        .Take(2)
                        .ToList();
                }

                todaysAssignments.Add(zoneId, chosenCrew);
                availableCrews.Remove(chosenCrew[0]);
                availableCrews.Remove(chosenCrew[1]);
            }
            // Any crews left unassigned after zone pairing get marked supplemental
            supplementalCrews.AddRange(availableCrews);
            previousDayPairings = todaysAssignments;
            return todaysAssignments;
        }

        // Resets available parks to be full lists of parks once a cycle completes
        internal void ResetCycle(DateTime newCycleStart)
        {
            cycleStartDate = newCycleStart;
            foreach (Zone zone in zones)
                remainingParks[zone.ZoneId] = new List<Site>(zone.Parks);

            workingDaysUsed = 0;
            pendingZones.Clear();
            foreach (Zone zone in zones)
                pendingZones.Add(zone.ZoneId);
        }

        // Checks condition to see if all crews have visited every park at least once
        public bool IsGenerationComplete()
        {
            List<string> totalParkList = new();
            foreach (Zone zone in zones)
                foreach (Site park in zone.Parks)
                    totalParkList.Add($"{zone.ZoneId}-{park.Park.Trim()}");

            if (totalParkList.Count == 0) return false;

            bool crewHistoryMatchesParkList = crewVisitHistory.Values.All(crewHistory =>
                totalParkList.All(park => crewHistory.Contains(park)));

            return crewHistoryMatchesParkList;
        }
    }
}
