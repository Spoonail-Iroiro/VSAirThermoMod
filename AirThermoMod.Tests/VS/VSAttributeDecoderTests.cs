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
        [TestMethod()]
        public void DecodeToTemperatureSamplesTest() {
            var decoded = VSAttributeDecoder.DecodeTemperatureSamples(
                new TreeAttribute {
                    ["times"] = new IntArrayAttribute([60, 120]),
                    ["temperatures"] = new DoubleArrayAttribute([5.5, 7.5])
                }
            );
            var expected = new List<TemperatureSample> { new(60, 5.5), new(120, 7.5) };
            decoded.Should().Equal(expected);
        }
    }
}