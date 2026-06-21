using System;
using System.Collections.Generic;
using System.Linq;

namespace Parks_Route_Planner
{
    internal class ConstraintValidator
    {
        private readonly List<ScheduleDay> _schedule;
        private readonly List<DateTime> _cycleStartDates;
        private readonly DateTime _mowEventAnchor;
        private readonly List<Zone> _zones;

        public ConstraintValidator(List<ScheduleDay> schedule, List<DateTime> cycleStartDates, DateTime mowEventAnchor, List<Zone> zones)
        {
            _schedule = schedule;
            _cycleStartDates = cycleStartDates;
            _mowEventAnchor = mowEventAnchor;
            _zones = zones;
        }

        public List<string> Validate()
        {
            List<string> violations = new();

            violations.AddRange(CheckNoBlockedWednesdays());
            violations.AddRange(CheckEveryParkPerCycle());
            violations.AddRange(CheckLargeParkCrewCount());
            violations.AddRange(CheckNoCrewDoubleAssigned());
            violations.AddRange(CheckZonePerDayLimit());

            return violations;
        }

        // Rule 1: No mow-event Wednesdays should appear in the schedule
        private List<string> CheckNoBlockedWednesdays()
        {
            List<string> violations = new();

            foreach (ScheduleDay day in _schedule)
            {
                DateTime date = DateTime.Parse(day.Date);
                if (!CalendarBuilder.isValidWorkingDay(date, _mowEventAnchor))
                {
                    violations.Add($"Blocked day scheduled: {date:dddd, MMMM d yyyy} is a mow event Wednesday and should not have assignments.");
                }
            }

            return violations;
        }

        // Rule 2: Every park must appear at least once in each cycle
        private List<string> CheckEveryParkPerCycle()
        {
            List<string> violations = new();

            List<string> allParks = _zones
                .SelectMany(z => z.Parks.Select(p => $"{z.ZoneId}-{p.Park.Trim()}"))
                .ToList();

            // Skip the last cycle — it is always partial (generation stops mid-cycle)
            for (int i = 0; i < _cycleStartDates.Count - 1; i++)
            {
                DateTime cycleStart = _cycleStartDates[i];
                DateTime cycleEnd = i + 1 < _cycleStartDates.Count
                    ? _cycleStartDates[i + 1].AddDays(-1)
                    : DateTime.Parse(_schedule[^1].Date);

                int cycleNumber = i + 1;

                HashSet<string> parksInCycle = _schedule
                    .Where(d =>
                    {
                        DateTime date = DateTime.Parse(d.Date);
                        return date >= cycleStart && date <= cycleEnd;
                    })
                    .SelectMany(d => d.Assignments)
                    .SelectMany(a => a.AssignedParks ?? new())
                    .Select(p =>
                    {
                        int zoneId = _zones
                            .FirstOrDefault(z => z.Parks.Any(pk => pk.Park.Trim() == p.Park.Trim()))
                            ?.ZoneId ?? 0;
                        return $"{zoneId}-{p.Park.Trim()}";
                    })
                    .ToHashSet();

                List<string> missing = allParks.Where(p => !parksInCycle.Contains(p)).ToList();
                foreach (string park in missing)
                {
                    string parkName = park.Split('-', 2).LastOrDefault() ?? park;
                    violations.Add($"Cycle {cycleNumber}: Park \"{parkName}\" was not visited during this cycle.");
                }
            }

            return violations;
        }

        // Rule 3: Large parks must always have exactly 2 crews assigned
        private List<string> CheckLargeParkCrewCount()
        {
            List<string> violations = new();

            HashSet<string> largeParks = _zones
                .SelectMany(z => z.Parks.Where(p => p.isLarge).Select(p => p.Park.Trim()))
                .ToHashSet();

            foreach (ScheduleDay day in _schedule)
            {
                DateTime date = DateTime.Parse(day.Date);

                // Group assignments by zone
                var byZone = day.Assignments
                    .GroupBy(a => a.AssignedZone?.ZoneId ?? -1)
                    .ToList();

                foreach (var zoneGroup in byZone)
                {
                    bool zoneHasLargePark = zoneGroup
                        .Any(a => a.AssignedParks?.Any(p => largeParks.Contains(p.Park.Trim())) == true);

                    if (zoneHasLargePark)
                    {
                        int crewCount = zoneGroup.Count();
                        if (crewCount != 2)
                        {
                            violations.Add($"{date:dddd, MMMM d yyyy}: Zone {zoneGroup.Key} has a large park but has {crewCount} crew(s) assigned instead of 2.");
                        }
                    }
                }
            }

            return violations;
        }

        // Rule 4: No crew should be assigned to more than one zone on the same day
        private List<string> CheckNoCrewDoubleAssigned()
        {
            List<string> violations = new();

            foreach (ScheduleDay day in _schedule)
            {
                DateTime date = DateTime.Parse(day.Date);

                var crewZones = day.Assignments
                    .GroupBy(a => a.AssignedCrew)
                    .Where(g => g.Select(a => a.AssignedZone?.ZoneId).Distinct().Count() > 1)
                    .ToList();

                foreach (var group in crewZones)
                {
                    string zoneList = string.Join(" and ", group.Select(a => $"Zone {a.AssignedZone?.ZoneId}").Distinct());
                    violations.Add($"{date:dddd, MMMM d yyyy}: Crew {group.Key} is assigned to both {zoneList} on the same day.");
                }
            }

            return violations;
        }

        // Rule 5: Zones worked per day should not exceed CrewCount / 2
        private List<string> CheckZonePerDayLimit()
        {
            List<string> violations = new();

            int crewCount = _zones.Count > 0
                ? _schedule
                    .SelectMany(d => d.Assignments)
                    .Select(a => a.AssignedCrew)
                    .Concat(_schedule.SelectMany(d => d.SupplementalCrews ?? new()))
                    .DefaultIfEmpty(0)
                    .Max()
                : 0;

            int maxZonesPerDay = crewCount / 2;
            if (maxZonesPerDay == 0) return violations;

            foreach (ScheduleDay day in _schedule)
            {
                DateTime date = DateTime.Parse(day.Date);

                int zonesWorked = day.Assignments
                    .Select(a => a.AssignedZone?.ZoneId)
                    .Distinct()
                    .Count();

                if (zonesWorked > maxZonesPerDay)
                {
                    violations.Add($"{date:dddd, MMMM d yyyy}: {zonesWorked} zones worked but maximum for crew count is {maxZonesPerDay}.");
                }
            }

            return violations;
        }
    }
}
