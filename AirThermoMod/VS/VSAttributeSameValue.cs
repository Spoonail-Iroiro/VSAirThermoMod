using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Datastructures;

namespace AirThermoMod.VS {
    internal class VSAttributeSameValue : EqualityComparer<IAttribute> {
        override public bool Equals(IAttribute x, IAttribute y) {
            if (ReferenceEquals(x, y)) return true;

            if (x is null || y is null) return false;

            if (x.GetType() != y.GetType()) return false;

            if (x is TreeAttribute tx) {
                var ty = y as TreeAttribute;

                return tx.Keys.SequenceEqual(ty.Keys) && tx.Values.SequenceEqual(ty.Values, new VSAttributeSameValue());
            }
            var xv = x.GetValue();
            var yv = y.GetValue();

            if (xv is Array xva) {
                var yva = yv as Array;

                var comp = new VSAttributeSameValue();

                if (xva.Length != yva.Length) return false;

                for (var i = 0; i < xva.Length; i++) {
                    var e1 = xva.GetValue(i);
                    var e2 = yva.GetValue(i);
                    if (e1 is IAttribute e1a) {
                        var e2a = e2 as IAttribute;
                        if (!comp.Equals(e1a, e2a)) return false;
                    }
                    else {
                        if (!e1.Equals(e2)) return false;
                    }
                }

                return true;
            }

            return xv.Equals(yv);
        }

        override public int GetHashCode(IAttribute obj) {
            return obj.GetHashCode();
        }
    }
}