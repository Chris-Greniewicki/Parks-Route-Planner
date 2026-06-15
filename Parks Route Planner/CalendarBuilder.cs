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
                Console.WriteLine(currentDate);
                if (currentDate.Year == 2027)
                {
                    endGeneration = true;
                }
            }
            return list;
        }
    }
}
