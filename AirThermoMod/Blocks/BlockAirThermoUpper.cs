using AirThermoMod.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AirThermoMod.Blocks {
    internal class BlockAirThermoUpper : Block {

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1) {
            var downPos = pos.DownCopy();
            if (world.BlockAccessor.GetBlock(downPos) is BlockAirThermo block) block.OnBlockBroken(world, downPos, byPlayer, dropQuantityMultiplier);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos) {
            var downPos = pos.DownCopy();
            if (world.BlockAccessor.GetBlock(downPos) is BlockAirThermo block) return block.OnPickBlock(world, downPos);
            return base.OnPickBlock(world, pos);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position.DownCopy()) is BEAirThermo be) {
                return be.Interact(world, byPlayer);
            }
            else {
                api.Logger.Warning("Couldn't find Block Entity (BEAirThermo). Have you loaded this save without mods?");
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override bool DoPartialSelection(IWorldAccessor world, BlockPos pos) {
            return true;
        }
    }
}
