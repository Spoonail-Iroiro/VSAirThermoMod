using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AirThermoMod.Config {
    public enum TemperatureUnitSetting {
        Unspecified,
        Celsius,
        Fahrenheit
    }


    public record class AirThermoModClientConfig {
        [JsonConverter(typeof(StringEnumConverter))]
        public TemperatureUnitSetting unitSetting { get; set; } = TemperatureUnitSetting.Unspecified;

        public static bool TryParseTemperatureUnitSetting(string input, out TemperatureUnitSetting unit) {
            var caseIgnored = input?.Trim().ToLowerInvariant();

            switch (caseIgnored) {
                case "c":
                case "celsius":
                    unit = TemperatureUnitSetting.Celsius;
                    return true;
                case "f":
                case "fahrenheit":
                    unit = TemperatureUnitSetting.Fahrenheit;
                    return true;
                case "u":
                case "unspecified":
                    unit = TemperatureUnitSetting.Unspecified;
                    return true;
                default:
                    unit = TemperatureUnitSetting.Unspecified;
                    return false;
            }
        }
    }
}
