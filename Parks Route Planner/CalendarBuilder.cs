using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    internal class CalendarBuilder
    {
        public static List<ScheduleDay> GenerateScheduleList(DateTime startDate) 
        {
            List<ScheduleDay> list = new List<ScheduleDay>();
            var currentDate = startDate;
            bool endGeneration = false;
            Console.WriteLine(currentDate);
            while(endGeneration == false){
                currentDate = currentDate.AddDays(1);
                if (currentDate.Year == 2027)
                {
                    endGeneration = true;
                }
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    Console.WriteLine("Skipping weekend");
                    continue;
                }
                Console.WriteLine(currentDate);
            }
            return list;
        }
    }
}
