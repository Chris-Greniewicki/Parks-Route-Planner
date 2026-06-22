using Parks_Route_Planner;
using System;
using System.IO;
using System.Text.Json;

string filePath = Path.Combine(AppContext.BaseDirectory, "config.json");
string jsonString = File.ReadAllText(filePath);
Config config = JsonSerializer.Deserialize<Config>(jsonString);

var editor = new ConfigEditor(config, filePath);
config = editor.Run();

int parkCount = config.Zones.Sum(Zone => Zone.Parks.Count);

string nextMowEventDate = config.NextMowEventDate;
List<Zone> zone = config.Zones;
int crewCount = config.Crews;

bool mowDateValid = DateTime.TryParse(nextMowEventDate, out DateTime mowEventDate);
bool startDateValid = DateTime.TryParse(config.StartDate, out DateTime startDate);

if (mowDateValid && startDateValid)
{
    List<ScheduleDay> schedule = new();
    List<DateTime> cycleStartDates = new();
    Scheduler process = new Scheduler(zone, crewCount, startDate, mowEventDate);
    DateTime currentDate = startDate;

    cycleStartDates.Add(startDate);

    while (!process.IsGenerationComplete())
    {
        currentDate = currentDate.AddDays(1);
        bool validWorkingDay = CalendarBuilder.isValidWorkingDay(currentDate, mowEventDate);
        if (!validWorkingDay)
            continue;

        if (process.remainingParks.Values.All(p => p.Count == 0))
        {
            DateTime nextCycleStart = currentDate.AddDays(1);
            while (nextCycleStart.DayOfWeek != DayOfWeek.Monday)
                nextCycleStart = nextCycleStart.AddDays(1);

            process.ResetCycle(nextCycleStart);
            cycleStartDates.Add(nextCycleStart);
            currentDate = nextCycleStart.AddDays(-1);
            continue;
        }

        ScheduleDay validDay = process.ProcessDay(currentDate.ToString());
        schedule.Add(validDay);
    }

    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    string outputFile = Path.Combine(desktopPath, $"Routes_{DateTime.Today:yyyy_MM_dd}.txt");
    RouteFileWriter.WriteRouteFile(schedule, cycleStartDates, outputFile);

    var validator = new ConstraintValidator(schedule, cycleStartDates, mowEventDate, zone);
    List<string> violations = validator.Validate();

    var display = new GenerationDisplay(schedule.Count, cycleStartDates.Count, parkCount, crewCount, outputFile);
    display.Play();

    if (violations.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"Note: {violations.Count} constraint violation(s) detected. Review output file.");
    }

    Console.WriteLine();
    Console.WriteLine($"File saved to: {outputFile}");
    Console.WriteLine();
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey(true);
}
else
{
    Console.WriteLine("Configuration is incomplete. Please set all required dates before generating.");
    Console.ReadKey(true);
}
