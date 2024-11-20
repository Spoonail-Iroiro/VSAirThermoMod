using AirThermoMod.Core;
using AirThermoMod.VS;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vintagestory.API.Datastructures;

namespace AirThermoMod.VS.Tests {
    [TestClass()]
    public class VSAttributeEncoderTests {
        private static IEnumerable<object[]> EncodeTestData {
            get {
                IEnumerable<object[]> hoge = new[]{
                    new object[] {
                        new TemperatureSample (60, 5.5),
                        new TreeAttribute { ["time"] = new IntAttribute(60), ["temperature"] = new DoubleAttribute(5.5) }
                    },
                    new object[] {
                        new List<TemperatureSample>() { new TemperatureSample (60, 5.5), new TemperatureSample (120, 7.5) },
                        new TreeArrayAttribute(new [] {
                            new TreeAttribute { ["time"] = new IntAttribute(60), ["temperature"] = new DoubleAttribute(5.5) },
                            new TreeAttribute { ["time"] = new IntAttribute(120), ["temperature"] = new DoubleAttribute(7.5) }
                        })
                    }
                };
                return hoge;
            }
        }

        [DynamicData(nameof(EncodeTestData))]
        [TestMethod()]
        public void EncodeTest(object src, IAttribute expectedDst) {
            var comp = new VSAttributeSameValue();
            var encoded = VSAttributeEncoder.Encode(src);
            encoded.Should().Be(expectedDst, comp);
        }
    }
}