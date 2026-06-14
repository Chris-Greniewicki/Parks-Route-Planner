using Parks_Route_Planner;
using System;
using System.IO;
using System.Text.Json;

string currentDir = AppContext.BaseDirectory;
string filePath = Path.Combine(currentDir, "config.json");
string jsonString = File.ReadAllText(filePath);

//Need to figure out class creation before this section will work
Config config = JsonSerializer.Deserialize<Config>(jsonString);

Console.WriteLine($"{config.Crews} Crews found");
Console.WriteLine($"{config.NextMowEventDate} set as next mow event date");
Console.WriteLine($"{config.Zones.Count} Zones found");