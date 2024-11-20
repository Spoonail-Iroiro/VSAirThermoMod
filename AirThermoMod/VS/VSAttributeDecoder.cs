using AirThermoMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;

namespace AirThermoMod.VS {
    internal static class VSAttributeDecoder {
        public static List<TemperatureSample> FromTemperatureSamplesSoA(int[] times, double[] temperatures) {
            var count = Math.Min(times.Length, temperatures.Length);

            return Enumerable.Range(0, count)
                    .Select(i => new TemperatureSample(times[i], temperatures[i]))
                    .ToList();
        }

        public static List<TemperatureSample> DecodeTemperatureSamples(TreeAttribute attr) {
            var timesAttr = attr.GetAttribute("times") as IntArrayAttribute;
            var temperaturesAttr = attr.GetAttribute("temperatures") as DoubleArrayAttribute;

            if (timesAttr == null || temperaturesAttr == null) {
                throw new ArgumentException("Invalid TreeAttribute for List<TemperatureSample>");
            }

            return FromTemperatureSamplesSoA(timesAttr.value, temperaturesAttr.value);
        }
    }
}