using AirThermoMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;

namespace AirThermoMod.VS {
    internal static class VSAttributeDecoder {

        // To automatically decode IAttribute to proper object, reflection is necessary
        // For now, decode supports only object with specified type
        public static T Decode<T>(TreeAttribute attr) {
            return (T)DecodeGeneralObject(typeof(T), attr);
        }

        public static object DecodeGeneralObject(Type targetType, TreeAttribute attr) {
            if (targetType == typeof(TemperatureSample)) {
                return new TemperatureSample { Time = attr.GetInt("time"), Temperature = attr.GetDouble("temperature") };
            }
            else {
                throw new NotImplementedException();
            }
        }
    }
}