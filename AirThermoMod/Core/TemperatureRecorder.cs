﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Util;

namespace AirThermoMod.Core {
    internal record class TemperatureSample(int Time, double Temperature);
    //    {
    //    // rounded total minutes
    //    public int Time { get; set; }
    //    public double Temperature { get; set; }
    //}

    internal class TemperatureRecorder {
        public List<TemperatureSample> TemperatureSamples { get; private set; } = new();

        public void SetSamples(IEnumerable<TemperatureSample> samples) {
            TemperatureSamples = samples.ToList();
        }

        public void AddSample(TemperatureSample sample) {
            TemperatureSamples.Add(sample);
        }

        public void Normalize() {
            TemperatureSamples = TemperatureSamples.DistinctBy(sample => sample.Time).ToList();
            TemperatureSamples.Sort((l, r) => l.Time - r.Time);
        }

        public void Extend(IEnumerable<TemperatureSample> samples) {
            TemperatureSamples.AddRange(samples);
        }

        public void CleanUpSamplesByMinTime(int minTime) {
            SetSamples(TemperatureSamples.Where(x => x.Time >= minTime));
        }

        public string SimpleDescription(int sampleLimit = 30) {
            var sb = new StringBuilder();
            if (TemperatureSamples.Count > sampleLimit) {
                sb.AppendLine($"Oh there're too many samples, more than {sampleLimit}");
                sb.AppendLine($"We have {TemperatureSamples.Count} samples");
                sb.AppendLine($"The first is [{TemperatureSamples[0].Time / 60.0} hours, {TemperatureSamples[0].Temperature}]");
                sb.AppendLine($"The last is [{TemperatureSamples[TemperatureSamples.Count - 1].Time / 60.0} hours, {TemperatureSamples[TemperatureSamples.Count - 1].Temperature}]");
            }
            else {
                TemperatureSamples.Select(sample => $"[{sample.Time / 60.0} hours, {sample.Temperature}]").Foreach(str => sb.Append(str));
            }
            return sb.ToString();
        }


    }
}