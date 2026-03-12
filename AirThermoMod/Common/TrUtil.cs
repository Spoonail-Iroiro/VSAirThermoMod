using AirThermoMod.Config;
using System.Collections.Generic;
using Vintagestory.API.Config;

namespace AirThermoMod.Common {
    internal class TrUtil {
        public static string LK(string key) {
            return AirThermoModModSystem.ModID + ":" + key;
        }
    }

    public static class TemperatureUnitSettingExtension {
        public static string Tr(this TemperatureUnitSetting unitSetting) {
            var found = TemperatureUnitSettingToLangKey.TryGetValue(unitSetting, out var langKey);
            if (!found) {
                langKey = "temperatureunitsetting-unknown";
            }

            return Lang.Get(TrUtil.LK(langKey!));
        }

        public static readonly Dictionary<TemperatureUnitSetting, string> TemperatureUnitSettingToLangKey = new() {
            [TemperatureUnitSetting.Celsius] = "temperatureunitsetting-celsius",
            [TemperatureUnitSetting.Fahrenheit] = "temperatureunitsetting-fahrenheit",
            [TemperatureUnitSetting.Unspecified] = "temperatureunitsetting-unspecified"
        };
    }
}


