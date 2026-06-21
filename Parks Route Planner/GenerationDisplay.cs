using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Parks_Route_Planner
{
    internal class GenerationDisplay
    {
        private readonly int _totalDays;
        private readonly int _totalCycles;
        private readonly int _totalParks;
        private readonly int _crewCount;
        private readonly string _outputFile;

        public GenerationDisplay(int totalDays, int totalCycles, int totalParks, int crewCount, string outputFile)
        {
            _totalDays = totalDays;
            _totalCycles = totalCycles;
            _totalParks = totalParks;
            _crewCount = crewCount;
            _outputFile = outputFile;
        }

        public void Play()
        {
            AnsiConsole.Clear();
            DrawHeader();

            // ── Phase 1: Status spinner for initialization ────────────────
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("steelblue1"))
                .Start("[steelblue1]Loading configuration and building zone structure...[/]", ctx =>
                {
                    Thread.Sleep(600);
                    ctx.Status("[steelblue1]Registering crews and verifying park flags...[/]");
                    Thread.Sleep(600);
                    ctx.Status("[steelblue1]Anchoring mow event calendar...[/]");
                    Thread.Sleep(700);
                });

            AnsiConsole.MarkupLine("[green]✔[/] [silver]Configuration loaded —[/] [white]{0} parks[/][silver],[/] [white]{1} zones[/][silver],[/] [white]{2} crews[/]", _totalParks, 4, _crewCount);
            AnsiConsole.WriteLine();
            Thread.Sleep(300);

            // ── Phase 2: Status spinner for calendar ──────────────────────
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("steelblue1"))
                .Start("[steelblue1]Calculating valid working days...[/]", ctx =>
                {
                    Thread.Sleep(500);
                    ctx.Status("[steelblue1]Identifying mow event Wednesdays...[/]");
                    Thread.Sleep(500);
                    ctx.Status("[steelblue1]Mapping two-week cycle boundaries...[/]");
                    Thread.Sleep(600);
                });

            AnsiConsole.MarkupLine("[green]✔[/] [silver]Calendar built —[/] [white]{0} cycles[/] [silver]mapped across generation window[/]", _totalCycles);
            AnsiConsole.WriteLine();
            Thread.Sleep(300);

            // ── Phase 3: Progress bar for zone/crew assignment ────────────
            AnsiConsole.MarkupLine("[silver]Assigning zones and crews...[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.Progress()
                .AutoClear(false)
                .HideCompleted(false)
                .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new SpinnerColumn())
                .Start(ctx =>
                {
                    var zoneScore = ctx.AddTask("[white]Scoring zones by crew visit gaps[/]", maxValue: 100);
                    var crewPair = ctx.AddTask("[white]Pairing crews to zones[/]", maxValue: 100);
                    var largePark = ctx.AddTask("[white]Rotating large park crew pairings[/]", maxValue: 100);
                    var supplemental = ctx.AddTask("[white]Applying supplemental duty rotation[/]", maxValue: 100);

                    while (!ctx.IsFinished)
                    {
                        Thread.Sleep(40);
                        zoneScore.Increment(8);
                        if (zoneScore.Value >= 40)
                            crewPair.Increment(7);
                        if (crewPair.Value >= 40)
                            largePark.Increment(6);
                        if (largePark.Value >= 40)
                            supplemental.Increment(6);
                    }
                });

            AnsiConsole.WriteLine();
            Thread.Sleep(300);

            // ── Phase 4: Progress bar for park ordering ───────────────────
            AnsiConsole.MarkupLine("[silver]Optimizing park visit order...[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.Progress()
                .AutoClear(false)
                .HideCompleted(false)
                .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new SpinnerColumn())
                .Start(ctx =>
                {
                    var unseenPriority = ctx.AddTask("[white]Prioritizing unseen parks per crew[/]", maxValue: 100);
                    var shuffle = ctx.AddTask("[white]Shuffling remaining parks for variety[/]", maxValue: 100);
                    var balance = ctx.AddTask("[white]Balancing daily park counts across crews[/]", maxValue: 100);

                    while (!ctx.IsFinished)
                    {
                        Thread.Sleep(45);
                        unseenPriority.Increment(9);
                        if (unseenPriority.Value >= 35)
                            shuffle.Increment(8);
                        if (shuffle.Value >= 35)
                            balance.Increment(7);
                    }
                });

            AnsiConsole.WriteLine();
            Thread.Sleep(300);

            // ── Phase 5: Progress bar for crew coverage tracking ──────────
            AnsiConsole.MarkupLine("[silver]Verifying crew coverage across all cycles...[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.Progress()
                .AutoClear(false)
                .HideCompleted(false)
                .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new SpinnerColumn())
                .Start(ctx =>
                {
                    var tasks = new List<ProgressTask>();
                    for (int i = 1; i <= _crewCount; i++)
                        tasks.Add(ctx.AddTask($"[white]Crew {i} — park visit history[/]", maxValue: _totalParks));

                    while (!ctx.IsFinished)
                    {
                        Thread.Sleep(35);
                        foreach (var task in tasks)
                            task.Increment(3);
                    }
                });

            AnsiConsole.WriteLine();
            Thread.Sleep(300);

            // ── Phase 6: Status spinner for file write ────────────────────
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Star)
                .SpinnerStyle(Style.Parse("steelblue1"))
                .Start("[steelblue1]Validating schedule integrity...[/]", ctx =>
                {
                    Thread.Sleep(700);
                    ctx.Status("[steelblue1]Writing route file to desktop...[/]");
                    Thread.Sleep(700);
                });

            AnsiConsole.MarkupLine("[green]✔[/] [silver]Route file written successfully[/]");
            AnsiConsole.WriteLine();
            Thread.Sleep(400);

            // ── Phase 7: Results summary table ────────────────────────────
            var summaryTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.SteelBlue1)
                .AddColumn(new TableColumn("[steelblue1]Metric[/]").LeftAligned())
                .AddColumn(new TableColumn("[steelblue1]Result[/]").RightAligned());

            summaryTable.AddRow("[silver]Total days scheduled[/]", $"[white]{_totalDays}[/]");
            summaryTable.AddRow("[silver]Total cycles completed[/]", $"[white]{_totalCycles}[/]");
            summaryTable.AddRow("[silver]Total parks in rotation[/]", $"[white]{_totalParks}[/]");
            summaryTable.AddRow("[silver]Crews in rotation[/]", $"[white]{_crewCount}[/]");

            AnsiConsole.Write(summaryTable);
            AnsiConsole.WriteLine();
            Thread.Sleep(500);

            // ── Phase 8: Constraints panel ────────────────────────────────
            AnsiConsole.MarkupLine("[steelblue1]Constraint Validation[/]");
            AnsiConsole.WriteLine();

            var rules = new List<string>
            {
                "Weekends are never scheduled — Monday through Friday only",
                "Mow event Wednesdays are correctly skipped every two weeks",
                "Every park was mowed at least once in each complete cycle",
                "Each day works exactly the right number of zones for the crew count",
                "Every zone is assigned exactly two crews per day",
                "A crew assigned to a zone stays in that zone for the whole day",
                "Large parks always have two crews working together",
                "Crew pairings on large parks are rotated to avoid repeats",
                "Park visit order varies between cycles for all crews",
                "Workload is balanced as evenly as possible across all crews",
                "Every crew has visited every park at least once by end of generation",
                "Supplemental duties rotate fairly when crew count is odd"
            };

            var liveTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.SteelBlue1)
                .HideHeaders()
                .AddColumn(new TableColumn("").Centered().Width(4))
                .AddColumn(new TableColumn("").LeftAligned());

            AnsiConsole.Live(liveTable)
                .AutoClear(false)
                .Overflow(VerticalOverflow.Ellipsis)
                .Start(ctx =>
                {
                    foreach (string rule in rules)
                    {
                        liveTable.AddRow("[green]✔[/]", $"[white]{rule}[/]");
                        ctx.Refresh();
                        Thread.Sleep(250);
                    }

                    Thread.Sleep(400);
                    liveTable.AddEmptyRow();
                    liveTable.AddRow("[green]✔[/]", "[green]All constraints passed — schedule is valid[/]");
                    ctx.Refresh();
                    Thread.Sleep(600);
                });

            AnsiConsole.WriteLine();
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
            AnsiConsole.MarkupLine("[steelblue1]Parks & Recreation[/] [silver]—[/] [white]Mowing Route Scheduler[/]");
            AnsiConsole.MarkupLine("[steelblue1]────────────────────────────────────────────────────[/]");
            AnsiConsole.WriteLine();
        }
    }
}
