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
bool boolResult = DateTime.TryParse(nextMowEventDate, out DateTime result);
if (boolResult)
{
    List<ScheduleDay> list = CalendarBuilder.GenerateScheduleList(startDate, result);
}
else
{
    Console.WriteLine("Next Mow Event Date not enetered properly");
}