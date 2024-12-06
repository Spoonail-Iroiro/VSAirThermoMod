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
    public class VSAttributeSameValueTests {
        public static IEnumerable<object[]> EqualData {
            get {
                var treeArray1 = new TreeAttribute[] {
                    new() {["att1"] = new IntAttribute(2)},
                    new() {["temp"] = new StringAttribute("ABC")},
                };

                var treeArray2 = new TreeAttribute[] {
                    new() {["att1"] = new IntAttribute(2)},
                    new() {["temp"] = new StringAttribute("ABC")},
                };

                return [
                    [new IntAttribute(2), new IntAttribute(2)],
                    [new DoubleAttribute(3.0), new DoubleAttribute(3.0)],
                    [new StringAttribute("abc"), new StringAttribute("abc")],
                    [new TreeAttribute { ["att1"] = new IntAttribute(2) }, new TreeAttribute { ["att1"] = new IntAttribute(2) }],
                    [new IntArrayAttribute([1, 2, 3]), new IntArrayAttribute([1, 2, 3])],
                    [new DoubleArrayAttribute([2.2, 4.4]), new DoubleArrayAttribute([2.2, 4.4])],
                    [new TreeArrayAttribute(treeArray1), new TreeArrayAttribute(treeArray2)]
                ];
            }
        }

        [TestMethod()]
        [DynamicData(nameof(EqualData))]
        public void EqualsTest(IAttribute at1, IAttribute at2) {
            var comp = new VSAttributeSameValue();

            comp.Equals(at1, at2).Should().BeTrue();
        }

        public static IEnumerable<object[]> NotEqualData {
            get {
                var treeArray1 = new TreeAttribute[] {
                    new() {["att1"] = new IntAttribute(2)},
                    new() {["temp"] = new StringAttribute("ABC")},
                };

                var treeArray2 = new TreeAttribute[] {
                    new() {["att1"] = new IntAttribute(2)},
                    new() {["temp"] = new StringAttribute("abc")},
                };

                var stTreeArray1 = new TreeAttribute[]
                {
                    new() {["att1"] = new IntAttribute(2)},
                    new() {["temp"] = new StringAttribute("ABC")}
                };

                var stTreeArray2 = new TreeAttribute[]
                {
                    new() {["att1"] = new IntAttribute(2)},
                };

                return [
                    [new IntAttribute(2), new IntAttribute(11)],
                    [new DoubleAttribute(3.0), new DoubleAttribute(3.5)],
                    [new StringAttribute("abc"), new StringAttribute("ABc")],
                    [new TreeAttribute { ["att1"] = new IntAttribute(2) }, new TreeAttribute { ["att1"] = new IntAttribute(3) }],
                    [new IntArrayAttribute([1, 2, 3]), new IntArrayAttribute([1, 5, 3])],
                    [new DoubleArrayAttribute([2.2, 4.4]), new DoubleArrayAttribute([2.2, 5.4])],
                    [new TreeArrayAttribute(treeArray1), new TreeArrayAttribute(treeArray2)],
                    [new TreeArrayAttribute(stTreeArray1), new TreeArrayAttribute(stTreeArray2)]
                ];
            }
        }

        [TestMethod()]
        [DynamicData(nameof(NotEqualData))]
        public void NotEqualsTest(IAttribute at1, IAttribute at2) {
            var comp = new VSAttributeSameValue();

            comp.Equals(at1, at2).Should().BeFalse();
        }
    }
}