using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Parks_Route_Planner
{
    internal class ConfigEditor
    {
        private Config _config;
        private readonly string _filePath;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public ConfigEditor(Config config, string filePath)
        {
            _config = config;
            _filePath = filePath;
        }

        // ─────────────────────────────────────────────
        //  Entry point — called from Program.cs
        // ─────────────────────────────────────────────
        public Config Run()
        {
            while (true)
            {
                AnsiConsole.Clear();
                DrawHeader();

                int totalParks = _config.Zones.Sum(z => z.Parks.Count);
                AnsiConsole.MarkupLine($"[silver]Config loaded:[/] [white]{_config.Zones.Count} zones[/][silver],[/] [white]{totalParks} parks[/][silver],[/] [white]{_config.Crews} crews[/]");
                AnsiConsole.MarkupLine($"[silver]Next mow event:[/] [white]{(string.IsNullOrWhiteSpace(_config.NextMowEventDate) ? "Not set" : _config.NextMowEventDate)}[/]");
                AnsiConsole.MarkupLine($"[silver]Schedule start date:[/] [white]{(string.IsNullOrWhiteSpace(_config.StartDate) ? "Not set" : _config.StartDate)}[/]");
                AnsiConsole.WriteLine();

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[steelblue1]What would you like to do?[/]")
                        .HighlightStyle(new Style(Color.Red))
                        .AddChoices(
                            "Generate Schedule",
                            "Manage Zones & Parks",
                            "Change Number of Crews",
                            "Change Mow Event Date",
                            "Change Start Date",
                            "About & Features",
                            "Exit"));

                switch (choice)
                {
                    case "Generate Schedule":
                        // Validate start date before generating
                        if (!TryGetValidStartDate())
                            continue;
                        return _config;

                    case "Manage Zones & Parks":
                        ManageZones();
                        break;

                    case "Change Number of Crews":
                        ChangeCrewCount();
                        break;

                    case "Change Mow Event Date":
                        ChangeMowEventDate();
                        break;

                    case "Change Start Date":
                        ChangeStartDate();
                        break;

                    case "About & Features":
                        FeaturesDisplay.Show();
                        break;

                    case "Exit":
                        AnsiConsole.Clear();
                        AnsiConsole.MarkupLine("[silver]Goodbye.[/]");
                        Environment.Exit(0);
                        break;
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Validate start date before generation
        //  Returns true if valid and ready to go
        //  Returns false if user needs to set/fix it
        // ─────────────────────────────────────────────
        private bool TryGetValidStartDate()
        {
            // Case 1: not set at all
            if (string.IsNullOrWhiteSpace(_config.StartDate))
            {
                AnsiConsole.Clear();
                DrawHeader();
                ShowError("No start date has been set. Please set a start date before generating a schedule.");
                ChangeStartDate();
                return false;
            }

            // Case 2: set but outdated
            if (DateTime.TryParse(_config.StartDate, out DateTime parsed) && parsed.Date < DateTime.Today)
            {
                AnsiConsole.Clear();
                DrawHeader();
                ShowError($"The start date {parsed:MMMM d, yyyy} is in the past and cannot be used. Please set a new start date.");
                ChangeStartDate();
                return false;
            }

            // Case 3: valid
            return true;
        }

        // ─────────────────────────────────────────────
        //  Zone list screen
        // ─────────────────────────────────────────────
        private void ManageZones()
        {
            while (true)
            {
                AnsiConsole.Clear();
                DrawHeader();
                AnsiConsole.MarkupLine("[steelblue1]Manage Zones & Parks[/]");
                AnsiConsole.MarkupLine("[silver]Pick a zone to manage its parks, or add a brand new zone.[/]");
                AnsiConsole.WriteLine();

                var choices = _config.Zones
                    .Select(z => $"Zone {z.ZoneId}  ({z.Parks.Count} park{(z.Parks.Count == 1 ? "" : "s")})")
                    .ToList();
                choices.Add("Add a new zone");
                choices.Add("Back to main menu");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[white]Choose a zone:[/]")
                        .HighlightStyle(new Style(Color.Red))
                        .AddChoices(choices));

                if (choice == "Back to main menu")
                    return;

                if (choice == "Add a new zone")
                {
                    AddZone();
                    continue;
                }

                int zoneId = int.Parse(choice.Split(' ')[1]);
                Zone zone = _config.Zones.First(z => z.ZoneId == zoneId);
                ManageParksInZone(zone);
            }
        }

        // ─────────────────────────────────────────────
        //  Add a new zone
        // ─────────────────────────────────────────────
        private void AddZone()
        {
            AnsiConsole.Clear();
            DrawHeader();
            AnsiConsole.MarkupLine("[steelblue1]Add a New Zone[/]");
            AnsiConsole.WriteLine();

            int nextId = _config.Zones.Any() ? _config.Zones.Max(z => z.ZoneId) + 1 : 1;
            AnsiConsole.MarkupLine($"[silver]The new zone will be assigned[/] [white]Zone {nextId}[/][silver]. You can add parks to it after creating it.[/]");
            AnsiConsole.WriteLine();

            bool confirm = AnsiConsole.Confirm($"Create Zone {nextId}?");
            if (!confirm) return;

            _config.Zones.Add(new Zone { ZoneId = nextId, Parks = new List<Site>() });
            SaveConfig();
            ShowSaved($"Zone {nextId} has been added. You can now add parks to it.");
        }

        // ─────────────────────────────────────────────
        //  Parks menu for a specific zone
        // ─────────────────────────────────────────────
        private void ManageParksInZone(Zone zone)
        {
            while (true)
            {
                AnsiConsole.Clear();
                DrawHeader();
                AnsiConsole.MarkupLine($"[steelblue1]Zone {zone.ZoneId}[/] [silver]— {zone.Parks.Count} park{(zone.Parks.Count == 1 ? "" : "s")}[/]");
                AnsiConsole.WriteLine();

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[white]What would you like to do in this zone?[/]")
                        .HighlightStyle(new Style(Color.Red))
                        .AddChoices(
                            "View all parks",
                            "Add a new park",
                            "Edit a park",
                            "Remove a park",
                            "Remove this entire zone",
                            "Back to zone list"));

                switch (choice)
                {
                    case "View all parks":
                        ViewParks(zone);
                        break;

                    case "Add a new park":
                        AddPark(zone);
                        break;

                    case "Edit a park":
                        EditPark(zone);
                        break;

                    case "Remove a park":
                        RemovePark(zone);
                        break;

                    case "Remove this entire zone":
                        if (RemoveZone(zone)) return;
                        break;

                    case "Back to zone list":
                        return;
                }
            }
        }

        // ─────────────────────────────────────────────
        //  View all parks in a zone
        // ─────────────────────────────────────────────
        private void ViewParks(Zone zone)
        {
            AnsiConsole.Clear();
            DrawHeader();
            AnsiConsole.MarkupLine($"[steelblue1]Zone {zone.ZoneId} — All Parks[/]");
            AnsiConsole.WriteLine();

            if (!zone.Parks.Any())
            {
                AnsiConsole.MarkupLine("[silver]This zone has no parks yet. Use \"Add a new park\" to get started.[/]");
            }
            else
            {
                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.BorderColor(Color.SteelBlue1);
                table.AddColumn(new TableColumn("[steelblue1]Park Name[/]").LeftAligned());
                table.AddColumn(new TableColumn("[steelblue1]Address[/]").LeftAligned());
                table.AddColumn(new TableColumn("[steelblue1]Needs Two Crews?[/]").Centered());

                foreach (var park in zone.Parks.OrderBy(p => p.Park))
                {
                    table.AddRow(
                        $"[white]{park.Park}[/]",
                        $"[silver]{park.Address}[/]",
                        park.isLarge ? "[red]Yes — large park[/]" : "[silver]No[/]"
                    );
                }

                AnsiConsole.Write(table);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[silver]Press any key to go back...[/]");
            Console.ReadKey(true);
        }

        // ─────────────────────────────────────────────
        //  Add a park to a zone
        // ─────────────────────────────────────────────
        private void AddPark(Zone zone)
        {
            AnsiConsole.Clear();
            DrawHeader();
            AnsiConsole.MarkupLine($"[steelblue1]Add a New Park to Zone {zone.ZoneId}[/]");
            AnsiConsole.MarkupLine("[silver]Press Enter with a blank field at any point to cancel and go back.[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.Write(new Panel(
                "[silver]Type the full name of the park exactly as it should appear on the route sheet.\nExample:[/] [white]Harkey Park[/]")
                .Header("[steelblue1] Park Name [/]")
                .BorderColor(Color.SteelBlue1));
            AnsiConsole.WriteLine();
            string name = AnsiConsole.Prompt(
                new TextPrompt<string>("[white]Park name:[/]")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowCancelled();
                return;
            }

            AnsiConsole.WriteLine();

            AnsiConsole.Write(new Panel(
                "[silver]Type the street address of the park. This appears on the route sheet so crews know where to go.\nExample:[/] [white]1708 SW Douglas Ave[/]")
                .Header("[steelblue1] Address [/]")
                .BorderColor(Color.SteelBlue1));
            AnsiConsole.WriteLine();
            string address = AnsiConsole.Prompt(
                new TextPrompt<string>("[white]Address:[/]")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(address))
            {
                ShowCancelled();
                return;
            }

            AnsiConsole.WriteLine();

            AnsiConsole.Write(new Panel(
                "[silver]A large park requires TWO crews working together on the same day because it is too big for one crew to finish alone.\nIf one crew can handle it in a day, choose No.[/]")
                .Header("[steelblue1] Does this park need two crews? [/]")
                .BorderColor(Color.SteelBlue1));
            AnsiConsole.WriteLine();

            string largeChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .HighlightStyle(new Style(Color.Red))
                    .AddChoices(
                        "No  — one crew can handle this park",
                        "Yes — this is a large park that needs two crews",
                        "Cancel — go back without saving"));

            if (largeChoice.StartsWith("Cancel"))
            {
                ShowCancelled();
                return;
            }

            bool isLarge = largeChoice.StartsWith("Yes");

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[steelblue1]─────────────────────────────────────────[/]");
            AnsiConsole.MarkupLine($"[steelblue1]Name:[/]    [white]{name}[/]");
            AnsiConsole.MarkupLine($"[steelblue1]Address:[/] [white]{address}[/]");
            AnsiConsole.MarkupLine($"[steelblue1]Large?[/]   [white]{(isLarge ? "Yes — needs two crews" : "No — one crew")}[/]");
            AnsiConsole.MarkupLine("[steelblue1]─────────────────────────────────────────[/]");
            AnsiConsole.WriteLine();

            bool confirm = AnsiConsole.Confirm("Add this park?");
            if (!confirm)
            {
                ShowCancelled();
                return;
            }

            zone.Parks.Add(new Site { Park = name, Address = address, isLarge = isLarge });
            SaveConfig();
            ShowSaved($"{name} has been added to Zone {zone.ZoneId}.");
        }

        // ─────────────────────────────────────────────
        //  Edit a park in a zone
        // ─────────────────────────────────────────────
        private void EditPark(Zone zone)
        {
            if (!zone.Parks.Any())
            {
                ShowError("This zone has no parks to edit yet.");
                return;
            }

            AnsiConsole.Clear();
            DrawHeader();
            AnsiConsole.MarkupLine($"[steelblue1]Edit a Park in Zone {zone.ZoneId}[/]");
            AnsiConsole.MarkupLine("[silver]Use the arrow keys to pick the park you want to change.[/]");
            AnsiConsole.WriteLine();

            var parkChoices = zone.Parks.Select(p => p.Park).ToList();
            parkChoices.Add("Back");

            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[white]Which park do you want to edit?[/]")
                    .HighlightStyle(new Style(Color.Red))
                    .AddChoices(parkChoices));

            if (selected == "Back") return;

            Site park = zone.Parks.First(p => p.Park == selected);
            bool moved = EditParkFields(zone, park);

            // If park was moved to another zone, return to zone list
            if (moved) return;
        }

        // ─────────────────────────────────────────────
        //  Edit individual fields of a park
        //  Returns true if the park was moved to another zone
        // ─────────────────────────────────────────────
        private bool EditParkFields(Zone zone, Site park)
        {
            while (true)
            {
                AnsiConsole.Clear();
                DrawHeader();
                AnsiConsole.MarkupLine($"[steelblue1]Editing:[/] [white]{park.Park}[/] [silver](Zone {zone.ZoneId})[/]");
                AnsiConsole.MarkupLine("[silver]Press Enter with a blank field to cancel any change and go back.[/]");
                AnsiConsole.WriteLine();

                string largeLabel = park.isLarge ? "[red]Yes — needs two crews[/]" : "[silver]No — one crew[/]";

                var choices = new List<string>
                {
                    $"Park name       [silver](currently:[/] [white]{park.Park}[/][silver])[/]",
                    $"Address         [silver](currently:[/] [white]{park.Address}[/][silver])[/]",
                    $"Needs two crews [silver](currently:[/] {largeLabel}[silver])[/]"
                };

                // Only show move option if there are other zones to move to
                if (_config.Zones.Count > 1)
                    choices.Add("Move to a different zone");

                choices.Add("Back");

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[white]Which part do you want to change?[/]")
                        .HighlightStyle(new Style(Color.Red))
                        .AddChoices(choices));

                if (choice == "Back") return false;

                if (choice.StartsWith("Park name"))
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(new Panel(
                        $"[silver]Type the new name for this park, or press Enter with nothing typed to cancel.\nCurrent name:[/] [white]{park.Park}[/]")
                        .Header("[steelblue1] Park Name [/]")
                        .BorderColor(Color.SteelBlue1));
                    AnsiConsole.WriteLine();
                    string newName = AnsiConsole.Prompt(
                        new TextPrompt<string>("[white]New park name:[/]")
                            .AllowEmpty());

                    if (string.IsNullOrWhiteSpace(newName))
                    {
                        ShowCancelled();
                        continue;
                    }

                    park.Park = newName;
                    SaveConfig();
                    ShowSaved($"Park name updated to: {newName}");
                }
                else if (choice.StartsWith("Address"))
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(new Panel(
                        $"[silver]Type the new street address, or press Enter with nothing typed to cancel.\nCurrent address:[/] [white]{park.Address}[/]")
                        .Header("[steelblue1] Address [/]")
                        .BorderColor(Color.SteelBlue1));
                    AnsiConsole.WriteLine();
                    string newAddress = AnsiConsole.Prompt(
                        new TextPrompt<string>("[white]New address:[/]")
                            .AllowEmpty());

                    if (string.IsNullOrWhiteSpace(newAddress))
                    {
                        ShowCancelled();
                        continue;
                    }

                    park.Address = newAddress;
                    SaveConfig();
                    ShowSaved($"Address updated to: {newAddress}");
                }
                else if (choice.StartsWith("Needs two crews"))
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Write(new Panel(
                        "[silver]A large park requires TWO crews working together on the same day.\nChange this if the park's staffing needs have changed.[/]")
                        .Header("[steelblue1] Needs Two Crews? [/]")
                        .BorderColor(Color.SteelBlue1));
                    AnsiConsole.WriteLine();

                    string largeChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .HighlightStyle(new Style(Color.Red))
                            .AddChoices(
                                "No  — one crew can handle this park",
                                "Yes — this is a large park that needs two crews",
                                "Cancel — go back without changing"));

                    if (largeChoice.StartsWith("Cancel"))
                    {
                        ShowCancelled();
                        continue;
                    }

                    park.isLarge = largeChoice.StartsWith("Yes");
                    SaveConfig();
                    ShowSaved($"{park.Park} is now marked as: {(park.isLarge ? "large — needs two crews" : "standard — one crew")}");
                }
                else if (choice.StartsWith("Move to a different zone"))
                {
                    AnsiConsole.Clear();
                    DrawHeader();
                    AnsiConsole.MarkupLine($"[steelblue1]Move {park.Park} to a Different Zone[/]");
                    AnsiConsole.MarkupLine($"[silver]Currently in Zone {zone.ZoneId}. Pick the zone you want to move it to.[/]");
                    AnsiConsole.WriteLine();

                    var zoneChoices = _config.Zones
                        .Where(z => z.ZoneId != zone.ZoneId)
                        .Select(z => $"Zone {z.ZoneId}  ({z.Parks.Count} park{(z.Parks.Count == 1 ? "" : "s")})")
                        .ToList();
                    zoneChoices.Add("Cancel — go back without moving");

                    string zoneChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[white]Which zone do you want to move this park to?[/]")
                            .HighlightStyle(new Style(Color.Red))
                            .AddChoices(zoneChoices));

                    if (zoneChoice.StartsWith("Cancel"))
                    {
                        ShowCancelled();
                        continue;
                    }

                    int destinationId = int.Parse(zoneChoice.Split(' ')[1]);
                    Zone destination = _config.Zones.First(z => z.ZoneId == destinationId);

                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[steelblue1]─────────────────────────────────────────[/]");
                    AnsiConsole.MarkupLine($"[steelblue1]Moving:[/]  [white]{park.Park}[/]");
                    AnsiConsole.MarkupLine($"[steelblue1]From:[/]    [white]Zone {zone.ZoneId}[/]");
                    AnsiConsole.MarkupLine($"[steelblue1]To:[/]      [white]Zone {destinationId}[/]");
                    AnsiConsole.MarkupLine("[steelblue1]─────────────────────────────────────────[/]");
                    AnsiConsole.WriteLine();

                    bool confirm = AnsiConsole.Confirm("Move this park?");
                    if (!confirm)
                    {
                        ShowCancelled();
                        continue;
                    }

                    // Perform the move
                    zone.Parks.Remove(park);
                    destination.Parks.Add(park);
                    SaveConfig();
                    ShowSaved($"{park.Park} has been successfully moved to Zone {destinationId}.");
                    return true; // Signal that park was moved — exit edit loop and zone menu
                }
            }
        }

        // ─────────────────────────────────────────────
        //  Remove a park from a zone
        // ─────────────────────────────────────────────
        private void RemovePark(Zone zone)
        {
            if (!zone.Parks.Any())
            {
                ShowError("This zone has no parks to remove.");
                return;
            }

            AnsiConsole.Clear();
            DrawHeader();
            AnsiConsole.MarkupLine($"[steelblue1]Remove a Park from Zone {zone.ZoneId}[/]");
            AnsiConsole.MarkupLine("[silver]Use the arrow keys to pick the park you want to remove.[/]");
            AnsiConsole.WriteLine();

            var parkChoices = zone.Parks.Select(p => p.Park).ToList();
            parkChoices.Add("Back");

            string selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[white]Which park do you want to remove?[/]")
                    .HighlightStyle(new Style(Color.Red))
                    .AddChoices(parkChoices));

            if (selected == "Back") return;

            Site park = zone.Parks.First(p => p.Park == selected);

            AnsiConsole.Clear();
            DrawHeader();
            AnsiConsole.WriteLine();

            AnsiConsole.Write(new Panel(
                $"[white]{park.Park}[/]\n[silver]{park.Address}[/]\n[silver]{(park.isLarge ? "Large park — needs two crews" : "Standard park — one crew")}[/]")
                .Header("[red] You are about to permanently remove this park [/]")
                .BorderColor(Color.Red));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[silver]This cannot be undone. The park will be removed from the schedule permanently.[/]");
            AnsiConsole.WriteLine();

            bool confirm = AnsiConsole.Confirm("[red]Are you sure you want to remove this park?[/]");
            if (!confirm)
            {
                ShowCancelled();
                return;
            }

            zone.Parks.Remove(park);
            SaveConfig();
            ShowSaved($"{selected} has been removed from Zone {zone.ZoneId}.");
        }

        // ─────────────────────────────────────────────
        //  Remove an entire zone
        // ─────────────────────────────────────────────
        private bool RemoveZone(Zone zone)
        {
            AnsiConsole.Clear();
            DrawHeader();
            AnsiConsole.WriteLine();

            AnsiConsole.Write(new Panel(
                $"[white]Zone {zone.ZoneId}[/] [silver]contains[/] [white]{zone.Parks.Count} park{(zone.Parks.Count == 1 ? "" : "s")}[/][silver].\nAll parks in this zone will also be permanently removed.[/]")
                .Header("[red] You are about to remove an entire zone [/]")
                .BorderColor(Color.Red));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[silver]This cannot be undone.[/]");
            AnsiConsole.WriteLine();

            bool confirm = AnsiConsole.Confirm($"[red]Are you sure you want to remove Zone {zone.ZoneId} and all its parks?[/]");
            if (!confirm)
            {
                ShowCancelled();
                return false;
            }

            _config.Zones.Remove(zone);
            SaveConfig();
            ShowSaved($"Zone {zone.ZoneId} and all its parks have been removed.");
            return true;
        }

        // ─────────────────────────────────────────────
        //  Change crew count
        // ─────────────────────────────────────────────
        private void ChangeCrewCount()
        {
            AnsiConsole.Clear();
            DrawHeader();
            AnsiConsole.MarkupLine("[steelblue1]Change Number of Crews[/]");
            AnsiConsole.MarkupLine("[silver]Press Enter with a blank field to cancel and go back.[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.Write(new Panel(
                $"[silver]This is the total number of mowing crews available each day.\nCurrently set to:[/] [white]{_config.Crews} crews[/]\n\n[silver]Each day, crews are split into pairs and assigned to zones.\nYou need at least 2 crews for the schedule to work.[/]")
                .Header("[steelblue1] Crew Count [/]")
                .BorderColor(Color.SteelBlue1));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[silver]Type the new number of crews and press Enter. Must be 2 or more.[/]");
            AnsiConsole.WriteLine();

            string raw = AnsiConsole.Prompt(
                new TextPrompt<string>("[white]Number of crews (or leave blank to cancel):[/]")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(raw))
            {
                ShowCancelled();
                return;
            }

            if (!int.TryParse(raw, out int newCount) || newCount < 2)
            {
                ShowError("Please enter a whole number of 2 or more. Nothing was changed.");
                return;
            }

            if (newCount == _config.Crews)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[silver]No change — crew count stays at[/] [white]{_config.Crews}[/][silver].[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[silver]Press any key to go back...[/]");
                Console.ReadKey(true);
                return;
            }

            _config.Crews = newCount;
            SaveConfig();
            ShowSaved($"Crew count updated to {newCount}.");
        }

        // ─────────────────────────────────────────────
        //  Change mow event date
        // ─────────────────────────────────────────────
        private void ChangeMowEventDate()
        {
            AnsiConsole.Clear();
            DrawHeader();
            AnsiConsole.MarkupLine("[steelblue1]Change Mow Event Date[/]");
            AnsiConsole.MarkupLine("[silver]Press Enter with a blank field to cancel and go back.[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.Write(new Panel(
                $"[silver]This is the date of the first Wednesday when crews attend the city mow event\ninstead of mowing parks. The schedule will skip this Wednesday and every\nother Wednesday after it automatically.\n\nCurrently set to:[/] [white]{(string.IsNullOrWhiteSpace(_config.NextMowEventDate) ? "Not set" : _config.NextMowEventDate)}[/]")
                .Header("[steelblue1] Mow Event Date [/]")
                .BorderColor(Color.SteelBlue1));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[silver]Type the date in this format: [/][white]M-D-YYYY[/]");
            AnsiConsole.MarkupLine("[silver]Example:[/] [white]6-17-2026[/]");
            AnsiConsole.MarkupLine("[silver]The date must be a Wednesday. Leave blank to cancel.[/]");
            AnsiConsole.WriteLine();

            string raw = AnsiConsole.Prompt(
                new TextPrompt<string>("[white]Mow event date (or leave blank to cancel):[/]")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(raw))
            {
                ShowCancelled();
                return;
            }

            if (!DateTime.TryParse(raw, out DateTime parsed))
            {
                ShowError("That doesn't look like a valid date. Use M-D-YYYY format, like 6-17-2026. Nothing was changed.");
                return;
            }

            if (parsed.DayOfWeek != DayOfWeek.Wednesday)
            {
                ShowError($"{parsed:MMMM d, yyyy} is a {parsed.DayOfWeek}. The mow event must fall on a Wednesday. Nothing was changed.");
                return;
            }

            _config.NextMowEventDate = raw;
            SaveConfig();
            ShowSaved($"Mow event date updated to {raw}.");
        }

        // ─────────────────────────────────────────────
        //  Change start date
        // ─────────────────────────────────────────────
        private void ChangeStartDate()
        {
            AnsiConsole.Clear();
            DrawHeader();
            AnsiConsole.MarkupLine("[steelblue1]Change Schedule Start Date[/]");
            AnsiConsole.MarkupLine("[silver]Press Enter with a blank field to cancel and go back.[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.Write(new Panel(
                $"[silver]This is the Monday that the schedule generation will begin from.\nThe date must be today or in the future, and must fall on a Monday.\n\nCurrently set to:[/] [white]{(string.IsNullOrWhiteSpace(_config.StartDate) ? "Not set" : _config.StartDate)}[/]")
                .Header("[steelblue1] Schedule Start Date [/]")
                .BorderColor(Color.SteelBlue1));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[silver]Type the date in this format: [/][white]M-D-YYYY[/]");
            AnsiConsole.MarkupLine("[silver]Example:[/] [white]6-23-2026[/]");
            AnsiConsole.MarkupLine("[silver]The date must be a Monday and cannot be in the past. Leave blank to cancel.[/]");
            AnsiConsole.WriteLine();

            string raw = AnsiConsole.Prompt(
                new TextPrompt<string>("[white]Start date (or leave blank to cancel):[/]")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(raw))
            {
                ShowCancelled();
                return;
            }

            if (!DateTime.TryParse(raw, out DateTime parsed))
            {
                ShowError("That doesn't look like a valid date. Use M-D-YYYY format, like 6-23-2026. Nothing was changed.");
                return;
            }

            if (parsed.DayOfWeek != DayOfWeek.Monday)
            {
                ShowError($"{parsed:MMMM d, yyyy} is a {parsed.DayOfWeek}. The start date must be a Monday. Nothing was changed.");
                return;
            }

            if (parsed.Date < DateTime.Today)
            {
                ShowError($"{parsed:MMMM d, yyyy} is in the past. The start date must be today or a future Monday. Nothing was changed.");
                return;
            }

            _config.StartDate = raw;
            SaveConfig();
            ShowSaved($"Schedule start date set to {raw}.");
        }

        // ─────────────────────────────────────────────
        //  Save config to disk
        // ─────────────────────────────────────────────
        private void SaveConfig()
        {
            string json = JsonSerializer.Serialize(_config, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }

        // ─────────────────────────────────────────────
        //  Shared UI helpers
        // ─────────────────────────────────────────────
        private static void DrawHeader()
        {
            AnsiConsole.Write(
                new FigletText("City of")
                    .LeftJustified()
                    .Color(Color.SteelBlue1));
            AnsiConsole.Write(
                new FigletText("Lawton")
                    .LeftJustified()
                    .Color(Color.Red));
            AnsiConsole.MarkupLine("[steelblue1]Parks & Recreation[/] [silver]—[/] [white]Mowing Route Scheduler[/]");
            AnsiConsole.MarkupLine("[steelblue1]────────────────────────────────────────────────────[/]");
            AnsiConsole.WriteLine();
        }

        private static void ShowSaved(string message)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel($"[white]{message}[/]")
                .BorderColor(Color.SteelBlue1));
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[silver]Press any key to continue...[/]");
            Console.ReadKey(true);
        }

        private static void ShowCancelled()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel("[silver]No changes were made.[/]")
                .BorderColor(Color.SteelBlue1));
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[silver]Press any key to go back...[/]");
            Console.ReadKey(true);
        }

        private static void ShowError(string message)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Panel($"[red]{message}[/]")
                .BorderColor(Color.Red));
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[silver]Press any key to go back...[/]");
            Console.ReadKey(true);
        }
    }
}
