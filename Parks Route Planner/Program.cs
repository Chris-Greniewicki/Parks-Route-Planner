using Parks_Route_Planner;
using System;
using System.IO;
using System.Text.Json;

//locate config.json file
string currentDir = AppContext.BaseDirectory;
string filePath = Path.Combine(currentDir, "config.json");
//Read and store json file
string jsonString = File.ReadAllText(filePath);

//Deserialize json file
Config config = JsonSerializer.Deserialize<Config>(jsonString);
//count parks across zones
int parkCount = config.Zones.Sum(Zone => Zone.Parks.Count);
//Run schedule generation
DateTime startDate = DateTime.Today;
string nextMowEventDate = config.NextMowEventDate;
List<Zone> zone = config.Zones;
int crewCount = config.Crews;
bool boolResult = DateTime.TryParse(nextMowEventDate, out DateTime result);
if (boolResult)
{
    List<ScheduleDay> schedule = new();
    List<DateTime> cycleStartDates = new();
    Scheduler process = new Scheduler(zone, crewCount, startDate, result);
    DateTime currentDate = startDate;

    DateTime cycleStartMonday = startDate;
    while (cycleStartMonday.DayOfWeek != DayOfWeek.Monday)
        cycleStartMonday = cycleStartMonday.AddDays(1);

    cycleStartDates.Add(cycleStartMonday);

    while (!process.IsGenerationComplete())
    {
        currentDate = currentDate.AddDays(1);
        bool validWorkingDay = CalendarBuilder.isValidWorkingDay(currentDate, result);
        if (!validWorkingDay)
            continue;

        if (currentDate.DayOfWeek == DayOfWeek.Monday &&
            (currentDate - cycleStartMonday).Days % 14 == 0 &&
            currentDate != cycleStartMonday)
        {
            process.ResetCycle(currentDate);
            cycleStartMonday = currentDate;
            cycleStartDates.Add(cycleStartMonday);
        }

        ScheduleDay validDay = process.ProcessDay(currentDate.ToString());
        schedule.Add(validDay);
    }

    Console.WriteLine($"Generation complete! {schedule.Count} days scheduled.");
    RouteFileWriter.WriteRouteFile(schedule, cycleStartDates);
    Console.WriteLine($"File saved to: Routes_{DateTime.Today:yyyy_MM_dd}.txt");
}
else
{
    Console.WriteLine("Next Mow Event Date not enetered properly");
}