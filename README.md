# Parks & Recreation Mowing Route Scheduler
![Parks Route Scheduler Banner](https://raw.githubusercontent.com/Chris-Greniewicki/Parks-Route-Planner/master/lawton_parks_scheduler_banner_wide.png)
### City of Lawton, Oklahoma — Parks & Recreation Department

A purpose-built scheduling tool that automatically generates optimized mowing route 
assignments for park maintenance crews. The program distributes parks across crews 
and zones over two-week cycles, enforces operational rules, tracks coverage, and 
produces a ready-to-use route file — all from a simple JSON config.

---

## Features

### Scheduling Engine
- Gap-scored zone selection prioritizes zones with the most unvisited parks each day
- Crew assignments rotate to maximize individual park coverage across all cycles
- Large parks are always assigned two crews, with pairings rotated to avoid repeats
- Park visit order is shuffled each cycle to keep routes varied and fresh
- Generation runs until every crew has visited every park at least once

### Calendar & Cycles
- Monday through Friday scheduling only — no weekends
- Mow event Wednesdays are automatically skipped every two weeks
- Two-week cycle structure with independent park coverage tracking per cycle
- Cycle boundaries anchored to the mow event date defined in config

### Crew Management
- Supports any number of crews with automatic zone workload adjustment
- Odd crew counts handled with strict round-robin supplemental duty rotation
- Unassigned crews are clearly marked as Supplemental Duties in the output file
- Workload balanced as evenly as possible across all active crews

### Config & Output
- Built-in console UI for managing zones, parks, crew count, and mow event date
- All config changes save instantly to config.json — no save button required
- Route file written to the desktop, named with the current date
- Output organized by cycle and day with zone, crew, and park breakdowns
- Automatic constraint validation runs after every generation

---

## Requirements
- Windows
- .NET 10

---

## Configuration
All scheduling parameters are managed through the built-in config editor at startup. 
The editor allows you to add, edit, and remove zones and parks, adjust crew count, 
and update the mow event date. Changes are saved immediately to `config.json`.

---

## Output
The generated route file is saved to the desktop as `Routes_YYYY_MM_DD.txt`. 
It is organized by cycle and day, showing each crew's zone assignment and park 
list for every working day in the schedule.

---

## Developer
Developed by **Christopher Greniewicki**  
Built for the City of Lawton — Parks & Recreation Department

---

## AI Disclosure
This application was developed with the assistance of [Claude](https://www.anthropic.com), 
an AI assistant made by Anthropic. All design decisions, requirements, testing, and 
direction were provided by the developer. Claude assisted with code generation, 
debugging, and implementation throughout the development process.

---
