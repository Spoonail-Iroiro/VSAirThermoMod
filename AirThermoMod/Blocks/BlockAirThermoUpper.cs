using AirThermoMod.BlockEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AirThermoMod.Blocks {
    internal class BlockAirThermoUpper : Block {

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1) {
            var block = world.BlockAccessor.GetBlock(pos.DownCopy()) as BlockAirThermo;
            if (block != null) block.OnBlockBroken(world, pos.DownCopy(), byPlayer, dropQuantityMultiplier);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos) {
            var block = world.BlockAccessor.GetBlock(pos.DownCopy()) as BlockAirThermo;
            if (block != null) return block.OnPickBlock(world, pos.DownCopy());
            return base.OnPickBlock(world, pos);
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel) {
            var block = world.BlockAccessor.GetBlock(blockSel.Position.DownCopy()) as BlockAirThermo;
            var be = world.BlockAccessor.GetBlockEntity(blockSel.Position.DownCopy()) as BEAirThermo;

            if (be != null) {
                return be.Interact(world, byPlayer);
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos) {
            return true;
        }
    }
}
