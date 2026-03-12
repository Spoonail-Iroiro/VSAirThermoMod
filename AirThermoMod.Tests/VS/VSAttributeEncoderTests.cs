using AirThermoMod.Core;
using FluentAssertions;
using Vintagestory.API.Datastructures;

namespace AirThermoMod.VS.Tests {
    [TestClass()]
    public class VSAttributeEncoderTests {
        private static IEnumerable<object[]> EncodeTemperatureSamplesTestData {
            get {
                IEnumerable<object[]> hoge = new[]{
                    new object[] {
                        new List<TemperatureSample>() { new TemperatureSample(60, 5.5), new TemperatureSample(120, 7.5) },
                        new TreeAttribute {
                            ["times"] = new IntArrayAttribute([60, 120]),
                            ["temperatures"] = new DoubleArrayAttribute([5.5,7.5])
                        }
                    }
                };
                return hoge;
            }
        }

        [DynamicData(nameof(EncodeTemperatureSamplesTestData))]
        [TestMethod()]
        public void EncodeTemperatureSamplesTest(object src, IAttribute expectedDst) {
            var comp = new VSAttributeSameValue();
            var encoded = VSAttributeEncoder.EncodeTemperatureSamples((List<TemperatureSample>)src);
            encoded.Should().Be(expectedDst, comp);
        }
    }
}