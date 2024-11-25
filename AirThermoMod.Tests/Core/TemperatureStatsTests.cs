using Microsoft.VisualStudio.TestTools.UnitTesting;
using AirThermoMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using AirThermoMod.Common;

namespace AirThermoMod.Core.Tests {
    [TestClass()]
    public class TemperatureStatsTests {
        public static VSTimeScale defaultTimeScale = new() { DaysPerMonth = 9, HoursPerDay = 24f };

        [TestMethod()]
        public void DailyMinAndMaxTest() {
            var samples = new List<TemperatureSample> {
                new TemperatureSample(60, 5.5),
                new TemperatureSample(120, 3.5),
                new TemperatureSample(180, 12.5),
                new TemperatureSample(1440, 5.5),
                new TemperatureSample(1500, 2.5),
                new TemperatureSample(2880, 4.5),
            };

            var stats = new TemperatureStats(defaultTimeScale);

            var result = stats.DailyMinAndMax(samples, "asc");

            result.Should().Equal(
                new DailyMinAndMaxResult(VSDateTime.FromDateTimeValue(defaultTimeScale, 0, 1, 1), 3.5, 12.5, 0.1, 1.0),
                new DailyMinAndMaxResult(VSDateTime.FromDateTimeValue(defaultTimeScale, 0, 1, 2), 2.5, 5.5, 0.0, 0.3),
                new DailyMinAndMaxResult(VSDateTime.FromDateTimeValue(defaultTimeScale, 0, 1, 3), 4.5, 4.5, 0.2, 0.2)
            );
        }

        [TestMethod()]
        public void DailyMinAndMaxDescTest() {
            var samples = new List<TemperatureSample> {
                new TemperatureSample(60, 5.5),
                new TemperatureSample(120, 3.5),
                new TemperatureSample(180, 12.5),
                new TemperatureSample(1440, 5.5),
                new TemperatureSample(1500, 2.5),
                new TemperatureSample(2880, 4.5),
            };

            var stats = new TemperatureStats(defaultTimeScale);

            var result = stats.DailyMinAndMax(samples, "desc");

            result.Should().Equal(
                new DailyMinAndMaxResult(VSDateTime.FromDateTimeValue(defaultTimeScale, 0, 1, 3), 4.5, 4.5, 0.2, 0.2),
                new DailyMinAndMaxResult(VSDateTime.FromDateTimeValue(defaultTimeScale, 0, 1, 2), 2.5, 5.5, 0.0, 0.3),
                new DailyMinAndMaxResult(VSDateTime.FromDateTimeValue(defaultTimeScale, 0, 1, 1), 3.5, 12.5, 0.1, 1.0)
            );
        }

        [TestMethod]
        public void DailyMinAndMaxZeroRangeTest() {
            var samples = new List<TemperatureSample> {
                new TemperatureSample(60, 4.5)
            };

            var stats = new TemperatureStats(defaultTimeScale);

            var result = stats.DailyMinAndMax(samples, "asc");

            result.Should().Equal(
                new DailyMinAndMaxResult(VSDateTime.FromDateTimeValue(defaultTimeScale, 0, 1, 1), 4.5, 4.5, 0.0, 1.0)
            );
        }

        [TestMethod]
        public void DailyMinAndMaxZeroSampleTest() {
            var samples = new List<TemperatureSample> {
            };

            var stats = new TemperatureStats(defaultTimeScale);

            var result = stats.DailyMinAndMax(samples, "asc");

            result.Should().Equal(
            );
        }

    }
}