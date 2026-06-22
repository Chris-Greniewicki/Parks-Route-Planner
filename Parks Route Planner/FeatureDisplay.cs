using Spectre.Console;
using System;

namespace Parks_Route_Planner
{
    internal class FeaturesDisplay
    {
        public static void Show()
        {
            AnsiConsole.Clear();
            DrawHeader();

            int width = Console.WindowWidth - 2;

            // ── Top description panel ─────────────────────────────────────
            WritePanel(width, new Panel(
                new Markup(
                    "[white]A fully automated park route scheduler for the City of Lawton Parks & Recreation department.[/]\n" +
                    "[silver]Generates complete crew assignments across all zones and parks, respects operational rules,\n" +
                    "tracks coverage across cycles, and produces a ready-to-use route file -- all from a simple config.[/]"))
                .Header("[steelblue1] About This Program [/]")
                .BorderColor(Color.SteelBlue1)
                .Padding(1, 1));

            AnsiConsole.WriteLine();

            // ── Category 1: Scheduling Engine ────────────────────────────
            WritePanel(width, new Panel(
                new Markup(
                    "[red]*[/] [white]Smart zone selection[/] [silver]-- zones with the most unvisited parks get prioritized each day[/]\n" +
                    "[red]*[/] [white]Gap-scored crew assignment[/] [silver]-- crews are sent to zones where they still have parks to see[/]\n" +
                    "[red]*[/] [white]Large park pairing[/] [silver]-- big parks always get two crews, and pairings rotate to avoid repeats[/]\n" +
                    "[red]*[/] [white]Unseen park priority[/] [silver]-- within a zone, each crew is assigned parks they haven't visited yet[/]\n" +
                    "[red]*[/] [white]Varied visit order[/] [silver]-- park order is shuffled each cycle so routes stay fresh[/]\n" +
                    "[red]*[/] [white]Full coverage guarantee[/] [silver]-- generation continues until every crew has visited every park at least once[/]"))
                .Header("[steelblue1] Scheduling Engine [/]")
                .BorderColor(Color.SteelBlue1)
                .Padding(1, 1));

            AnsiConsole.WriteLine();

            // ── Category 2: Calendar & Cycles ────────────────────────────
            WritePanel(width, new Panel(
                new Markup(
                    "[red]*[/] [white]Weekday-only scheduling[/] [silver]-- Monday through Friday, no exceptions[/]\n" +
                    "[red]*[/] [white]Automatic mow event skipping[/] [silver]-- the configured Wednesday is skipped every two weeks throughout the entire schedule[/]\n" +
                    "[red]*[/] [white]Two-week cycle structure[/] [silver]-- all parks are expected to be covered within each 14-day window[/]\n" +
                    "[red]*[/] [white]Cycle boundary anchoring[/] [silver]-- cycles always start on a Monday, anchored to the mow event date in config[/]\n" +
                    "[red]*[/] [white]Independent cycle tracking[/] [silver]-- park coverage resets each cycle and is tracked and reported separately[/]"))
                .Header("[steelblue1] Calendar & Cycles [/]")
                .BorderColor(Color.SteelBlue1)
                .Padding(1, 1));

            AnsiConsole.WriteLine();

            // ── Category 3: Crew Management ───────────────────────────────
            WritePanel(width, new Panel(
                new Markup(
                    "[red]*[/] [white]Flexible crew count[/] [silver]-- supports any number of crews, adjusting zone workload automatically[/]\n" +
                    "[red]*[/] [white]Odd crew handling[/] [silver]-- when crew count is odd, one crew rotates to supplemental duties each day[/]\n" +
                    "[red]*[/] [white]Strict round-robin rotation[/] [silver]-- supplemental duty assignment cycles evenly so no crew is left out too often[/]\n" +
                    "[red]*[/] [white]Unassigned crew marking[/] [silver]-- any crew without a zone assignment is clearly labeled in the output file[/]\n" +
                    "[red]*[/] [white]Balanced workload[/] [silver]-- the scheduler distributes parks as evenly as possible across all active crews[/]\n" +
                    "[red]*[/] [white]Coverage fairness[/] [silver]-- supplemental crews still accumulate park visits when active so coverage stays balanced[/]"))
                .Header("[steelblue1] Crew Management [/]")
                .BorderColor(Color.SteelBlue1)
                .Padding(1, 1));

            AnsiConsole.WriteLine();

            // ── Category 4: Config & Output ───────────────────────────────
            WritePanel(width, new Panel(
                new Markup(
                    "[red]*[/] [white]Built-in config editor[/] [silver]-- manage zones, parks, crew count, and mow event date without touching any files[/]\n" +
                    "[red]*[/] [white]Instant saves[/] [silver]-- every change writes to config.json immediately, no save button needed[/]\n" +
                    "[red]*[/] [white]Full park management[/] [silver]-- add, edit, or remove parks and zones at any time with guided prompts[/]\n" +
                    "[red]*[/] [white]Park zone transfer[/] [silver]-- move any park to a different zone at any time directly from the park editor[/]\n" +
                    "[red]*[/] [white]Start date control[/] [silver]-- set the exact Monday you want schedule generation to begin from, with automatic validation to prevent outdated dates[/]\n" +
                    "[red]*[/] [white]Desktop output[/] [silver]-- the finished route file is saved directly to the desktop, named with today's date[/]\n" +
                    "[red]*[/] [white]Readable route format[/] [silver]-- output is organized by cycle and day, showing zones, crews, and park lists clearly[/]\n" +
                    "[red]*[/] [white]Automatic constraint validation[/] [silver]-- every rule is checked after generation and violations are reported before the file is saved[/]\n" +
                    "[red]*[/] [white]Theatrical generation display[/] [silver]-- animated progress bars and spinners show every step of the scheduling process[/]"))
                .Header("[steelblue1] Config & Output [/]")
                .BorderColor(Color.SteelBlue1)
                .Padding(1, 1));

            AnsiConsole.WriteLine();

            // ── Developer panel ───────────────────────────────────────────
            WritePanel(width, new Panel(
                new Markup(
                    "[silver]Developed by[/]  [white]Christopher Greniewicki[/]\n" +
                    "[silver]Built for[/]     [white]City of Lawton -- Parks & Recreation Department[/]\n" +
                    "[silver]Released[/]      [white]June 20, 2026[/]"))
                .Header("[red] Developer [/]")
                .BorderColor(Color.Red)
                .Padding(1, 1));

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[silver]Press any key to return to the main menu...[/]");
            Console.ReadKey(true);
        }

        private static void WritePanel(int width, Panel panel)
        {
            var grid = new Grid();
            grid.AddColumn(new GridColumn().Width(width));
            grid.AddRow(panel.Expand());
            AnsiConsole.Write(grid);
        }

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
            AnsiConsole.MarkupLine("[steelblue1]Parks & Recreation[/] [silver]--[/] [white]Park Route Scheduler[/]");
            AnsiConsole.MarkupLine("[steelblue1]────────────────────────────────────────────────────[/]");
            AnsiConsole.WriteLine();
        }
    }
}
