using AirThermoMod.Core;
using AirThermoMod.VS;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;

namespace AirThermoMod.VS.Tests {

    [TestClass()]
    public class VSAttributeDecoderTests {

        private static EqualityComparer<TemperatureSample> compValueSame = EqualityComparer<TemperatureSample>.Create((x, y) => x.Time == y.Time && x.Temperature == y.Temperature);


        [TestMethod()]
        public void DecodeToTemperatureSampleTest() {
            var decoded = VSAttributeDecoder.Decode<TemperatureSample>(new TreeAttribute { ["time"] = new IntAttribute(60), ["temperature"] = new DoubleAttribute(5.5) });
            var expected = new TemperatureSample(60, 5.5);
            decoded.Should().Be(expected: expected, comparer: compValueSame);
        }

        [TestMethod()]
        public void DecodeTreeArrayAttributeTest() {
            var taattr = new TreeArrayAttribute(new[] {
                new TreeAttribute { ["time"] = new IntAttribute(60), ["temperature"] = new DoubleAttribute(5.5) },
                new TreeAttribute { ["time"] = new IntAttribute(120), ["temperature"] = new DoubleAttribute(7.5) }
            });

            var decoded = taattr.value.Select(attr => VSAttributeDecoder.Decode<TemperatureSample>(attr)).ToList();
            decoded.Should().Equal(new List<TemperatureSample> {
                new(60, 5.5),
                new(120, 7.5)
            }, compValueSame.Equals);
        }
    }
}