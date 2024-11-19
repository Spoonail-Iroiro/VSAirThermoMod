using AirThermoMod.Common;
using AirThermoMod.Core;
using AirThermoMod.VS;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace AirThermoMod.Test
{
    [TestClass]
    public sealed class LearningTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var rounded = TimeUtil.ToRoundedTotalMinutesN(1.0);
            Assert.AreEqual(60, rounded);

            var dt = new VSDateTime(9, 24.0f, TimeSpan.FromHours(1.0));
            var d = GameMath.SmoothStep(0.5);
            Console.WriteLine(d);
        }

        [TestMethod]
        public void SmoothStepTest()
        {
            var d = GameMath.SmoothStep(0.5);
            Assert.AreEqual(0.5, d);
        }

        [TestMethod]
        public void TreeAttributeTest1()
        {
            var ta = VSAttributeEncoder.Encode(new List<TemperatureSample> { new TemperatureSample { Time = 60, Temperature = 15 } });
            Console.WriteLine($"{ta}");
        }

        [TestMethod]
        public void TreeAttributeTest2()
        {
            var ta = new TreeAttribute();
            ta.SetDouble("hoge", 5.0);
        }
    }
}
