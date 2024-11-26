using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirThermoMod.Config {
    public record class AirThermoModConfig(int samplingIntervalMinutes = 60);
}
