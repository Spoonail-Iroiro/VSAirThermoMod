using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;

namespace AirThermoMod.VS {
    internal class VSAttributeSameValue : EqualityComparer<IAttribute> {
        // A custom equality comparer for IAttribute objects
        override public bool Equals(IAttribute x, IAttribute y) {
            if (ReferenceEquals(x, y)) return true;

            if (x is null || y is null) return false;

            if (x.GetType() != y.GetType()) return false;

            // TreeAttributes are equal when their Keys and Values are equal
            if (x is TreeAttribute tx) {
                var ty = y as TreeAttribute;

                return tx.Keys.SequenceEqual(ty.Keys) && tx.Values.SequenceEqual(ty.Values, new VSAttributeSameValue());
            }

            // Now they are not TreeAttribute, so let's look their value
            var xv = x.GetValue();
            var yv = y.GetValue();

            // When the values are arrays (IntArrayAttributes, DoubleArrayAttributes, TreeArrayAttributes...)
            if (xv is Array xva) {
                var yva = yv as Array;

                var comp = new VSAttributeSameValue();

                // If they have different length, they are not equal
                if (xva.Length != yva.Length) return false;

                // For each element...
                for (var i = 0; i < xva.Length; i++) {
                    var e1 = xva.GetValue(i);
                    var e2 = yva.GetValue(i);
                    if (e1 is IAttribute e1a) {
                        // if the elements are IAttribute, compare them by VSAttributeSameValue (recursively)
                        var e2a = e2 as IAttribute;
                        if (!comp.Equals(e1a, e2a)) return false;
                    }
                    else {
                        // else, just compare by Equals
                        if (!e1.Equals(e2)) return false;
                    }
                }

                // If their all elements are equal, x and y are equal
                return true;
            }

            // If x and y are not *ArrayAttributes, just compare their values
            return xv.Equals(yv);
        }

        override public int GetHashCode(IAttribute obj) {
            return obj.GetHashCode();
        }
    }
}