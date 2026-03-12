using AirThermoMod.Common;
using ProtoBuf;
using System.ComponentModel;
using Vintagestory.API.Util;

namespace AirThermoMod.Tests {
    [ProtoContract(SkipConstructor = true)]
    record class DefaultValueClass(
        [property: ProtoMember(1, IsRequired = false)][property: DefaultValue(4)] int? intVall
    );

    [ProtoContract()]
    record class TestRecordClass(
        [property: ProtoMember(1)] int? intVall,
        [property: ProtoMember(2)] string? strVal,
        [property: ProtoMember(3)] string? addedVal
    ) {
        public TestRecordClass() : this(2, "str", "added") {

        }
    }

    [TestClass]
    public sealed class DevTests {
        [TestMethod]
        public void ProtobufTest() {
            var record = new DefaultValueClass(2);

            var serialized = SerializerUtil.Serialize(record);

            File.WriteAllBytes("testdata.proto", serialized);

            var deserialized = SerializerUtil.Deserialize<DefaultValueClass>(serialized);

            Console.Write(deserialized);

        }

        [TestMethod]
        public void ProtobufMissingMembertest() {
            var data = File.ReadAllBytes("testdata.proto");

            var deserialized = SerializerUtil.Deserialize<TestRecordClass>(data);

            Console.Write($"{deserialized}");
        }



        [TestMethod]
        public void TempTest() {
            //var samples = Enumerable.Range(0, 10)
            //    .Select(i => (time: 60 * i, temperature: 6.5 * i));
            var samples = Enumerable.Empty<(int time, double temperature)>();

            var minAll = samples.Select(samp => (double?)samp.temperature).Min();
            var maxAll = samples.Select(samp => (double?)samp.temperature).Max();
            // ItemStack
            //AssetLocation.Create()
            //ItemCheese
            //ItemStack

        }

        [TestMethod]
        public void TreeAttributeTest2() {
            var val = -3.8545;
            Console.WriteLine($"{val}");
            Console.WriteLine($"{TemperatureUtil.ToFahrenheight(val)}");
            Console.WriteLine($"{val:F1}");
            Console.WriteLine($"{TemperatureUtil.ToFahrenheight(val):F1}");
        }
    }
}