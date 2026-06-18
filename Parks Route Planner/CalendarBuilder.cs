using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace Parks_Route_Planner
{
    internal class CalendarBuilder
    {
       
        public static bool isValidWorkingDay(DateTime date, DateTime mowEventAnchor)
        {
            if (date.DayOfWeek == DayOfWeek.Wednesday)
            {
                DateTime endDate = date.Date;
                TimeSpan difference = endDate - mowEventAnchor.Date;
                double totalDays = difference.TotalDays;
                int divisible = Convert.ToInt32(totalDays);
                int remainder = divisible % 14;
                if (remainder == 0)
                {
                    return false;
                }

            }
            else if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }
            return true;
        }
        public static int GetWorkingDaysInCycle(DateTime cycleStart, DateTime mowEventAnchor)
        {
            int validDaysCounter = 0;
            for (DateTime cycleStartDate = cycleStart; cycleStartDate <= cycleStart.AddDays(14); cycleStartDate = cycleStartDate.AddDays(1))
            {
               bool isValid = isValidWorkingDay(cycleStartDate, mowEventAnchor);
                if (isValid)
                {
                    validDaysCounter = validDaysCounter + 1;
                }
            }
            return validDaysCounter;
        }
    }
}
