using AirThermoMod.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;
using Vintagestory.Server;

namespace AirThermoMod.VS
{
    internal static class VSAttributeEncoder
    {
        public static IAttribute Encode(object obj)
        {
            // TODO: dict
            if (obj is IEnumerable<object> list)
            {
                return EncodeIEnumerable(list);
            }
            else
            {
                return EncodeGeneralObject(obj);
            }
        }

        private static TreeAttribute EncodeGeneralObject(object obj)
        {
            if (obj is TemperatureSample sample)
            {
                var tree = new TreeAttribute();
                tree.SetInt("time", sample.Time);
                tree.SetDouble("temperature", sample.Temperature);
                return tree;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static TreeArrayAttribute EncodeIEnumerable(IEnumerable<object> list)
        {
            // It's TreeArray attribute, array in array is not allowed, so parsing elements as general object here
            // Todo: dict
            return new TreeArrayAttribute(list.Select(x => EncodeGeneralObject(x)).ToArray());
        }

    }
    internal static class VSAttributeDecoder
    {

        // To automatically decode IAttribute to proper object, reflection is necessary
        // For now, decode supports only object with specified type
        public static T Decode<T>(TreeAttribute attr)
        {
            return (T)DecodeGeneralObject(typeof(T), attr);
        }

        public static object DecodeGeneralObject(Type targetType, TreeAttribute attr)
        {
            if (targetType == typeof(TemperatureSample))
            {
                return new TemperatureSample { Time = attr.GetInt("time"), Temperature = attr.GetDouble("temperature") };
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        //public static TreeAttribute ToTreeAttribute(TemperatureSample sample)
        //{
        //    var tree = new TreeAttribute();
        //    tree.SetInt("time", sample.Time);
        //    tree.SetDouble("temperature", sample.Temperature);
        //    return tree;
        //}

        //public static TreeAttribute ToTreeAttribute<T>(List<T> list)
        //{
        //    var arrOftree = list.Select(x => 2).ToArray();

        //    return new TreeArrayAttribute(arrOftree);

        //}

        //public static T FromTreeAttribute<T>(TreeAttribute tree)
        //{
        //    if (typeof(T) == typeof(TemperatureSample))
        //    {
        //        return (T)(object)new TemperatureSample { Time = tree.GetInt("time"), Temperature = tree.GetDouble("temperature") };
        //    }
        //    else
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

    }
}
