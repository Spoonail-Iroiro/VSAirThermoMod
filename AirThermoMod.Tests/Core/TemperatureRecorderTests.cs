using Microsoft.VisualStudio.TestTools.UnitTesting;
using AirThermoMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace AirThermoMod.Core.Tests {
    [TestClass()]
    public class TemperatureRecorderTests {
        [TestMethod()]
        public void SetSamplesTest() {
            var recorder = new TemperatureRecorder();
            recorder.SetSamples([new(60, 5.5), new(120, 7.5)]);
            recorder.TemperatureSamples.Should().Equal(
                new List<TemperatureSample> {
                    new(60, 5.5),
                    new(120, 7.5)
                }
            );

            recorder.AddSample(new(180, 9.5));

            recorder.SetSamples([new(90, 15.5), new(180, 17.5)]);
            recorder.TemperatureSamples.Should().Equal(
                new List<TemperatureSample> {
                    new(90, 15.5),
                    new(180, 17.5)
                }
            );
        }

        [TestMethod()]
        public void AddSampleTest() {
            var recorder = new TemperatureRecorder();

            recorder.AddSample(new(60, 5.5));
            recorder.AddSample(new(90, 7.5));

            recorder.TemperatureSamples.Should().Equal(
                new List<TemperatureSample> {
                    new(60, 5.5),
                    new(90, 7.5)
                }
            );

            recorder.SetSamples(new List<TemperatureSample> { new(50, 2.2) });

            recorder.AddSample(new(100, 4.4));
            recorder.AddSample(new(150, 6.6));

            recorder.TemperatureSamples.Should().Equal(
                new List<TemperatureSample> {
                    new(50, 2.2),
                    new(100, 4.4),
                    new(150, 6.6)
                }
            );
        }

        [TestMethod()]
        public void NormalizeTest() {
            var recorder = new TemperatureRecorder();

            recorder.AddSample(new(60, 5.5));
            recorder.AddSample(new(90, 7.5));
            recorder.AddSample(new(90, 8.5));
            recorder.AddSample(new(30, 3.5));

            recorder.Normalize();

            recorder.TemperatureSamples.Should().Equal(
                new List<TemperatureSample> {
                    new(30, 3.5),
                    new(60, 5.5),
                    new(90, 7.5),
                }
            );
        }

        [TestMethod()]
        public void ExtendTest() {
            var recorder = new TemperatureRecorder();

            recorder.AddSample(new(30, 7.5));
            recorder.AddSample(new(60, 5.5));

            recorder.Extend(new List<TemperatureSample> {
                new(50, 9.5),
                new(20, 10.5)
            });

            recorder.TemperatureSamples.Should().Equal(
                new List<TemperatureSample> {
                    new(30, 7.5),
                    new(60, 5.5),
                    new(50, 9.5),
                    new(20, 10.5)
                }
            );
        }


        [Ignore()]
        [TestMethod()]
        public void CleanUpSamplesByMinTimeTest() {
            Assert.Fail();
        }
    }
}