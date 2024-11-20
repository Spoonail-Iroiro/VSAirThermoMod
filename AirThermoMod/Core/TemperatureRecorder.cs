using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Util;

namespace AirThermoMod.Core {
    internal class TemperatureSample {
        // rounded total minutes
        public int Time { get; set; }
        public double Temperature { get; set; }
    }

    internal class TemperatureRecorder {
        public List<TemperatureSample> TemperatureSamples { get; private set; } = new();

        public void SetSamples(IEnumerable<TemperatureSample> samples) {
            TemperatureSamples = samples.ToList();
        }

        public void AddSample(TemperatureSample sample) {
            TemperatureSamples.Add(sample);
        }

        public void CleanUpSamplesByMinTime(int minTime) {
            SetSamples(TemperatureSamples.Where(x => x.Time >= minTime));
        }

        public string SimpleDescription() {
            var sb = new StringBuilder();
            TemperatureSamples.Select(sample => $"[{sample.Time / 60.0} hours, {sample.Temperature}]").Foreach(str => sb.Append(str));
            return sb.ToString();
        }
    }
}