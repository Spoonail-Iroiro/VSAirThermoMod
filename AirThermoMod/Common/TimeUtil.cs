using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace AirThermoMod.Common {
    public class VSTimeScale {
        public int DaysPerMonth { get; set; }

        public float HoursPerDay { get; set; }
    }

    public class VSDateTime {
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

        public VSDateTime(VSTimeScale ts, TimeSpan timeSpan) {
            TimeSpan = timeSpan;
            DaysPerMonth = ts.DaysPerMonth;
            HoursPerDay = ts.HoursPerDay;
        }

        public VSDateTime(IGameCalendar calendar, TimeSpan timeSpan) : this(new VSTimeScale { DaysPerMonth = calendar.DaysPerMonth, HoursPerDay = calendar.HoursPerDay }, timeSpan) {

        }

        public static VSDateTime FromDateTimeValue(
            VSTimeScale ts,
            int year = 0,
            int month1 = 1,
            int day1 = 1,
            int hour = 0,
            int minute = 0,
            int second = 0
        ) {
            int month0 = month1 - 1;
            int day0 = day1 - 1;

            double totalSeconds = 0;
            totalSeconds += second;
            totalSeconds += minute * 60;
            totalSeconds += hour * 60 * 60;
            totalSeconds += day0 * 60 * 60 * ts.HoursPerDay;
            totalSeconds += month0 * 60 * 60 * ts.HoursPerDay * ts.DaysPerMonth;
            totalSeconds += year * 60 * 60 * ts.HoursPerDay * ts.DaysPerMonth * 12;

            return new VSDateTime(ts, TimeSpan.FromSeconds(totalSeconds));
        }

        public static VSDateTime FromDateTimeValue(
            IGameCalendar calendar,
            int year = 0,
            int month1 = 1,
            int day1 = 1,
            int hour = 0,
            int minute = 0,
            int second = 0
        ) {
            var ts = new VSTimeScale {
                DaysPerMonth = calendar.DaysPerMonth,
                HoursPerDay = calendar.HoursPerDay
            };

            return FromDateTimeValue(
                ts,
                year,
                month1,
                day1,
                hour,
                minute,
                second
            );
        }

        public static VSDateTime FromYearRel(VSTimeScale ts, double yearRel) {
            double totalHours = 1.0 * ts.HoursPerDay * ts.DaysPerMonth * 12.0 * yearRel;

            return new VSDateTime(ts, TimeSpan.FromHours(totalHours));
        }

        public static VSDateTime FromYearRel(IGameCalendar calendar, double yearRel) {
            return FromYearRel(new VSTimeScale { DaysPerMonth = calendar.DaysPerMonth, HoursPerDay = calendar.HoursPerDay }, yearRel);
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

        public static double TotalYearsToTotalHours(IGameCalendar calendar, double totalYears) {
            var dt = VSDateTime.FromYearRel(calendar, totalYears);

            return dt.TimeSpan.TotalHours;
        }


    }
}