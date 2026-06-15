using System;
using System.Collections.Generic;
using System.Text;

namespace Parks_Route_Planner
{
    internal class CalendarBuilder
    {
        public static List<ScheduleDay> GenerateScheduleList(DateTime startDate, DateTime nextMowEventDate) 
        {
            List<ScheduleDay> list = new List<ScheduleDay>();
            var currentDate = startDate;
            bool endGeneration = false;
            Console.WriteLine(currentDate);
            while (endGeneration == false){
                currentDate = currentDate.AddDays(1);
                if (currentDate.Year == 2027)
                {
                    endGeneration = true;
                }
                if (currentDate.DayOfWeek == DayOfWeek.Wednesday)
                {
                    DateTime endDate = currentDate.Date;
                    TimeSpan difference = endDate - nextMowEventDate.Date;
                    double totalDays = difference.TotalDays;
                    int divisible = Convert.ToInt32(totalDays);
                    int remainder = divisible % 14;
                    if (remainder == 0)
                    {
                        Console.WriteLine("Skipping Mow Event Wednesday");
                        continue;
                    }

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
