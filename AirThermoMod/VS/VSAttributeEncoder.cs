using AirThermoMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;
using Vintagestory.Server;

namespace AirThermoMod.VS {
    internal static class VSAttributeEncoder {

        /// <summary>
        /// Convert List<TemperatureSamples> (array of structs) to (int[], double[]) (struct of arrays)
        /// </summary>
        /// <param name="samples"></param>
        /// <returns>(times, temperatures)</returns>
        public static Tuple<int[], double[]> ToTemperatureSamplesSoA(List<TemperatureSample> samples) {
            var count = samples.Count;

            var times = new int[count];
            var temperatures = new double[count];

            for (var i = 0; i < count; ++i) {
                times[i] = samples[i].Time;
                temperatures[i] = samples[i].Temperature;
            }

            return Tuple.Create(times, temperatures);
        }

        /// <summary>
        /// Encode List<TemperatureSample> to Attribute of Vintage Story
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static IAttribute EncodeTemperatureSamples(List<TemperatureSample> samples) {
            var soa = ToTemperatureSamplesSoA(samples);

            var tree = new TreeAttribute {
                ["times"] = new IntArrayAttribute(soa.Item1),
                ["temperatures"] = new DoubleArrayAttribute(soa.Item2)
            };

            return tree;
        }
    }
}