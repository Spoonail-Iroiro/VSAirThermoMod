namespace AirThermoMod.Common {
    internal class TrUtil {
        public static string LocalKey(string key) {
            return AirThermoModModSystem.ModID + ":" + key;
        }
    }
}
