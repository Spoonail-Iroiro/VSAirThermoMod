using AirThermoMod.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirThermoMod.Core {
    record class DailyMinAndMaxResult(VSDateTime DateTime, double Min, double Max, double RateMin, double RateMax);

    internal class TemperatureStats {
        VSTimeScale timeScale;

        public TemperatureStats(VSTimeScale timeScale) {
            this.timeScale = timeScale;
        }

        public IEnumerable<DailyMinAndMaxResult> DailyMinAndMax(List<TemperatureSample> samples, string order) {
            if (samples.Count == 0) return Enumerable.Empty<DailyMinAndMaxResult>();

            if (order != "asc" && order != "desc") throw new ArgumentException("Order must be asc or desc");

            var resultMinAndMax = samples.GroupBy(
                    s => Math.Floor(new VSDateTime(timeScale, TimeSpan.FromMinutes(s.Time)).TotalDays),
                    (roundedTotalDays, samples) => (
                        dateTime: VSDateTime.FromTotalDays(timeScale, 1.0 * roundedTotalDays),
                        min: samples.Select(s => s.Temperature).Min(),
                        max: samples.Select(s => s.Temperature).Max()
                    )
                );

            var minAllTime = resultMinAndMax.Min(stat => stat.min);
            var maxAllTime = resultMinAndMax.Max(stat => stat.max);
            var rangeAllTime = maxAllTime - minAllTime;

            var result = resultMinAndMax
                .Select(stat => new DailyMinAndMaxResult(stat.dateTime, stat.min, stat.max, rangeAllTime > 0.01 ? (stat.min - minAllTime) / rangeAllTime : 0.0, (rangeAllTime > 0.01) ? (stat.max - minAllTime) / rangeAllTime : 1.0))
                .OrderBy(stat => order == "asc" ? stat.DateTime.TimeSpan : -stat.DateTime.TimeSpan);

            return result;
        }
    }
}
