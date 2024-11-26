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