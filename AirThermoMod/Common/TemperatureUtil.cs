using System.Collections.Generic;
using System.Linq;

namespace AirThermoMod.Common {
    internal class TemperatureUtil {
        public static double ToFahrenheight(double celsiusTemperature) {
            return celsiusTemperature * 9.0 / 5.0 + 32.0;
        }
    }
}
