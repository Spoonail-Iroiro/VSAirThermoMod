using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AirThermoMod.Config {
    public enum UnitSetting {
        Celsius,
        Fahrenheit
    }

    public record class AirThermoModClientConfig {
        [JsonConverter(typeof(StringEnumConverter))]
        public UnitSetting unitSetting { get; set; } = UnitSetting.Celsius;
    }
}
