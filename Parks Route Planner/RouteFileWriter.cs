using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    internal class RouteFileWriter
    {
        public static void WriteRouteFile(List<ScheduleDay> schedule, List<DateTime> cycleStartDates, string outputPath)
        {
            using StreamWriter writer = File.CreateText(outputPath);
            int cycleNumber = 0;
            HashSet<string> cycleParks = new();

            // Build full park list from all assignments as reference
            HashSet<string> allParks = schedule
                .SelectMany(d => d.Assignments)
                .SelectMany(a => a.AssignedParks ?? new())
                .Select(p => p.Park)
                .ToHashSet();

            foreach (ScheduleDay day in schedule)
            {
                DateTime currentDate = DateTime.Parse(day.Date);

                if (cycleStartDates.Contains(currentDate))
                {
                    if (cycleNumber > 0)
                    {
                        var missing = allParks.Except(cycleParks).ToList();
                        string missingStr = missing.Count > 0 ? $" MISSING: {string.Join(", ", missing)}" : "";
                        writer.WriteLine($"  Parks covered: {cycleParks.Count}{missingStr}");
                    }

                    cycleParks.Clear();
                    cycleNumber++;
                    DateTime cycleStart = currentDate;
                    DateTime cycleEnd = cycleStartDates.Count > cycleNumber
                        ? cycleStartDates[cycleNumber].AddDays(-1)
                        : DateTime.Parse(schedule[^1].Date);

                    writer.WriteLine("");
                    writer.WriteLine("========================================");
                    writer.WriteLine($"  Cycle {cycleNumber}: {cycleStart:MMMM d, yyyy} – {cycleEnd:MMMM d, yyyy}");
                    writer.WriteLine("========================================");
                }

                foreach (Assignment a in day.Assignments)
                    foreach (Site p in a.AssignedParks ?? new())
                        cycleParks.Add(p.Park);

                writer.WriteLine("");
                writer.WriteLine(DateTime.Parse(day.Date).ToString("dddd, MMMM dd yyyy"));

                foreach (Assignment assignment in day.Assignments)
                {
                    if (assignment.AssignedParks == null || assignment.AssignedParks.Count == 0)
                        continue;
                    writer.WriteLine($"  Zone {assignment.AssignedZone.ZoneId} — Crew {assignment.AssignedCrew}");
                    foreach (Site park in assignment.AssignedParks)
                        writer.WriteLine($"     - {park.Park}");
                }

                if (day.SupplementalCrews != null && day.SupplementalCrews.Count > 0)
                {
                    writer.WriteLine("");
                    foreach (int crew in day.SupplementalCrews)
                        writer.WriteLine($"  Crew {crew} — Supplemental Duties");
                }
            }

            var finalMissing = allParks.Except(cycleParks).ToList();
            string finalMissingStr = finalMissing.Count > 0 ? $" MISSING: {string.Join(", ", finalMissing)}" : "";
            writer.WriteLine($"  Parks covered: {cycleParks.Count}{finalMissingStr}");
        }
    }
}
