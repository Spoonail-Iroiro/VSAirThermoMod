﻿using AirThermoMod.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;

namespace AirThermoMod.Items {
    internal class ItemRecordedChartPaper : Item {
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo) {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
            var srcPos = inSlot.Itemstack.Attributes.GetBlockPos("srcpos");
            if (srcPos != null) {
                dsc.AppendLine(Lang.Get(TrUtil.LocalKey("src-pos-info"), Math.Floor(srcPos.X - api.World.DefaultSpawnPosition.X) + 1, srcPos.Y, Math.Floor(srcPos.Z - api.World.DefaultSpawnPosition.Z) + 1));
            }
            else {
            }
        }
    }
}
