using Parks_Route_Planner;
using System;
using System.IO;
using System.Text.Json;

string currentDir = AppContext.BaseDirectory;
string filePath = Path.Combine(currentDir, "config.json");
string jsonString = File.ReadAllText(filePath);

Config config = JsonSerializer.Deserialize<Config>(jsonString);

//test of basic data retrieval
Console.WriteLine($"{config.Crews} Crews found");
Console.WriteLine($"{config.NextMowEventDate} set as next mow event date");
Console.WriteLine($"{config.Zones.Count} Zones found");
//test of ability to count parks across zones
int parkCount = config.Zones.Sum(Zone => Zone.Parks.Count);
Console.WriteLine($"{parkCount} Parks found");
//testing simple enum selection
Console.WriteLine($"{Crew.Crew2} selected");
//testing ability to set properties of a new object
Assignment test = new Assignment();
test.AssignedCrew = Crew.Crew2;
Console.WriteLine($"{test.AssignedCrew} assigned");
//Console.WriteLine($"{}");
//Console.WriteLine($"{}");