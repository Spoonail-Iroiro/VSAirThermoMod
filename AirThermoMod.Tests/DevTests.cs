using AirThermoMod.Common;
using AirThermoMod.Core;
using AirThermoMod.VS;
using FluentAssertions;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace AirThermoMod.Tests {
    [TestClass]
    public sealed class DevTests {
        [TestMethod]
        public void SmoothStepTest() {
            var d = GameMath.SmoothStep(0.5);
            d.Should().Be(0.5);
        }

        [TestMethod]
        public void TreeAttributeTest2() {
            var ta = new TreeAttribute();
            ta.SetDouble("hoge", 5.0);
        }
    }
}