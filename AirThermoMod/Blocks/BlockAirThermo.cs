using AirThermoMod.BlockEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace AirThermoMod.Blocks {
    internal class BlockAirThermo : Block {

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null) {
            base.OnBlockPlaced(world, blockPos, byItemStack);

            var toPlaceBlock = world.GetBlock(new AssetLocation("airthermomod:airthermoupper-" + Variant["orientation"]));

            world.BlockAccessor.SetBlock(toPlaceBlock.BlockId, blockPos.UpCopy());

            var be = GetBlockEntity<BEAirThermo>(blockPos);
            be?.OnBlockPlaced();
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1) {
            var upBlock = api.World.BlockAccessor.GetBlock(pos.UpCopy());
            if (upBlock.Code.Path == "airthermoupper-" + Variant["orientation"]) {
                world.BlockAccessor.SetBlock(0, pos.UpCopy());
            }

            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }

        public override bool CanPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref string failureCode) {
            if (!base.CanPlaceBlock(world, byPlayer, blockSel, ref failureCode)) return false;

            BlockSelection bs = blockSel.Clone();
            bs.Position = blockSel.Position.UpCopy();
            if (!base.CanPlaceBlock(world, byPlayer, bs, ref failureCode)) return false;

            return true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
            var be = GetBlockEntity<BEAirThermo>(blockSel.Position);
            if (be != null) {
                return be.Interact(world, byPlayer);
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}