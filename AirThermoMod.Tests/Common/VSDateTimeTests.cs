using Microsoft.VisualStudio.TestTools.UnitTesting;
using AirThermoMod.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace AirThermoMod.Common.Tests {
    [TestClass()]
    public class VSDateTimeTests {
        static VSTimeScale defaultScale = new VSTimeScale { DaysPerMonth = 9, HoursPerDay = 24f };

        public static IEnumerable<object[]> EqualsData {
            get {
                return [
                    [new VSDateTime(defaultScale, TimeSpan.FromHours(24.5)), new VSDateTime(defaultScale, TimeSpan.FromHours(24.5))],
                    [new VSDateTime(defaultScale, TimeSpan.FromHours(24.5)), VSDateTime.FromDateTimeValue(defaultScale, 0, 1, 2, 0, 30, 0)]
                ];
            }
        }

        public static IEnumerable<object[]> NotEqualsData {
            get {
                return [
                    [new VSDateTime(defaultScale, TimeSpan.FromHours(24.5)), new VSDateTime(defaultScale, TimeSpan.FromHours(24.6))],
                    [new VSDateTime(defaultScale, TimeSpan.FromHours(24)), new VSDateTime(new VSTimeScale { DaysPerMonth = 8, HoursPerDay = 23.5f }, TimeSpan.FromHours(24))]
                ];
            }
        }

        [Ignore]
        [TestMethod()]
        public void VSDateTimeTest() {
            Assert.Fail();
        }

        [Ignore]
        [TestMethod()]
        public void VSDateTimeTest1() {
            Assert.Fail();
        }

        [Ignore]
        [TestMethod()]
        public void FromDateTimeValueTest() {
            Assert.Fail();
        }

        [Ignore]
        [TestMethod()]
        public void FromDateTimeValueTest1() {
            Assert.Fail();
        }

        [Ignore]
        [TestMethod()]
        public void FromYearRelTest() {
            Assert.Fail();
        }

        [Ignore]
        [TestMethod()]
        public void FromYearRelTest1() {
            Assert.Fail();
        }

        [Ignore]
        [TestMethod()]
        public void FromTotalDaysTest() {
            Assert.Fail();
        }

        [Ignore]
        [TestMethod()]
        public void PrettyDateTest() {
            Assert.Fail();
        }

        [DynamicData(nameof(EqualsData))]
        [TestMethod()]
        public void EqualsTest(VSDateTime left, VSDateTime right) {
            left.Should().Be(right);
        }

        [DynamicData(nameof(NotEqualsData))]
        [TestMethod()]
        public void NotEqualsTest(VSDateTime left, VSDateTime right) {
            left.Should().NotBe(right);
        }

        [DynamicData(nameof(EqualsData))]
        [TestMethod()]
        public void GetHashCodeEqualsTest(VSDateTime left, VSDateTime right) {
            left.GetHashCode().Should().Be(right.GetHashCode());
        }

        [DynamicData(nameof(NotEqualsData))]
        [TestMethod()]
        public void GetHashCodeNotEqualsTest(VSDateTime left, VSDateTime right) {
            left.GetHashCode().Should().NotBe(right.GetHashCode());
        }
    }
}