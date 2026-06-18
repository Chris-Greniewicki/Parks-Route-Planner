using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    internal class RouteFileWriter
    {
        public static void WriteRouteFile(List<ScheduleDay> schedule)
        {
            string fileName = $"Routes_{DateTime.Today:yyyy_MM_dd}.txt";
            using StreamWriter writer = File.CreateText(fileName);
            foreach (ScheduleDay day in schedule)
            {
                writer.WriteLine("");
                writer.WriteLine(DateTime.Parse(day.Date).ToString("dddd, MMMM dd yyyy"));
                foreach (Assignment assignment in day.Assignments)
                {
                    writer.WriteLine($"  Zone {assignment.AssignedZone.ZoneId} — Crew {assignment.AssignedCrew}");
                    foreach (Site park in assignment.AssignedParks)
                    {
                        writer.WriteLine($"     - {park.Park}");
                    }
                }
            }
        }
    }
}
