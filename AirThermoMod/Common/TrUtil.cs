using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirThermoMod.Common {
    internal class TrUtil {
        public static string LocalKey(string key) {
            return AirThermoModModSystem.ModID + ":" + key;
        }
    }
}
