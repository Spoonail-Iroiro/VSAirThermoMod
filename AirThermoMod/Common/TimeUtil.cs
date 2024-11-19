using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace AirThermoMod.Common {
    internal class VSDateTime {
        public int DaysPerMonth { get; private set; }

        public float HoursPerDay { get; private set; }

        public TimeSpan TimeSpan { get; private set; }

        public int DaysPerYear => DaysPerMonth * 12;

        public int MonthsPerYear => DaysPerYear / DaysPerMonth;

        public double TotalDays => TimeSpan.TotalHours / (double)HoursPerDay;

        public int Year => (int)(TotalDays / DaysPerYear);

        public int Month => (int)Math.Ceiling(YearRel * MonthsPerYear);

        public int Day => (int)(TotalDays % DaysPerMonth) + 1;

        public int Hour => (int)(TimeSpan.TotalHours % (double)HoursPerDay);

        public int Minute => (int)(TimeSpan.TotalMinutes % 60.0);

        public int Second => (int)(TimeSpan.TotalSeconds % 60.0);

        public float YearRel => (float)(GameMath.Mod(TotalDays, DaysPerYear) / DaysPerYear);

        public EnumMonth MonthName => (EnumMonth)Month;

        public VSDateTime(int vsDaysPerMonth, float vsHoursPerDay, TimeSpan timeSpan) {
            TimeSpan = timeSpan;
            DaysPerMonth = vsDaysPerMonth;
            HoursPerDay = vsHoursPerDay;
        }

        public VSDateTime(IGameCalendar calendar, TimeSpan timeSpan) : this(calendar.DaysPerMonth, calendar.HoursPerDay, timeSpan) {

        }

        public string PrettyDate() {
            return Lang.Get("dateformat", Day, Lang.Get("month-" + MonthName), Year.ToString("0"), Hour.ToString("00"), Minute.ToString("00"));
        }

    }

    internal class TimeUtil {
        public static int MINUTES_PER_HOUR = 60;

        private static int Round(double d) {
            return (int)Math.Round(d);
        }

        public static int MinutesPerDay(float vsHourPerDay) {
            return MINUTES_PER_HOUR * Round(vsHourPerDay);
        }

        public static int ToRoundedTotalMinutesN(double totalHours) {
            // Round, not Floor, because when Floor used totalMinutesN -> totalHours -> totalMinutesN cannot be recovered correctly.
            return Round(totalHours * MINUTES_PER_HOUR);
        }

        public static double RoundedTotalMinutesToTotalHours(int totalMinutesN) {
            return 1.0 * totalMinutesN / MINUTES_PER_HOUR;
        }


    }
}