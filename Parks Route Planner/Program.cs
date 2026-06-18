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
    Scheduler process = new Scheduler(zone, crewCount, startDate, result);
    DateTime currentDate = startDate;
    while (!process.IsGenerationComplete())
    {
        currentDate = currentDate.AddDays(1);
        bool validWorkingDay = CalendarBuilder.isValidWorkingDay(currentDate, result);
        if (!validWorkingDay)
        {
            continue;
        }
        ScheduleDay validDay = process.ProcessDay(currentDate.ToString());
        schedule.Add(validDay);
    }
    Console.WriteLine($"Generation complete! {schedule.Count} days scheduled.");
}
else
{
    Console.WriteLine("Next Mow Event Date not enetered properly");
}
