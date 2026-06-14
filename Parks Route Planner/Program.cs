using Parks_Route_Planner;
using System;
using System.IO;
using System.Text.Json;

string currentDir = AppContext.BaseDirectory;
string filePath = Path.Combine(currentDir, "config.json");
string jsonString = File.ReadAllText(filePath);

//Need to figure out class creation before this section will work
//config config = JsonSerializer.Deserialize<config>(jsonString);

Console.WriteLine(filePath);
//Console.WriteLine($"Testing words: {config.Sites}");