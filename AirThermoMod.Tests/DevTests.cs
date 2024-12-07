using AirThermoMod.Common;
using AirThermoMod.Core;
using AirThermoMod.VS;
using FluentAssertions;
using ProtoBuf;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.Client;
using Vintagestory.Client.NoObf;

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
        }

        [TestMethod]
        public void TreeAttributeTest2() {
            var ta = new TreeAttribute();
            ta.SetDouble("hoge", 5.0);
        }
    }
}