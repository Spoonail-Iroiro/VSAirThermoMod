using AirThermoMod.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace AirThermoMod.Blocks {
    internal class BlockAirThermo : Block {
        public AssetLocation UpperBlockCode => new AssetLocation("airthermomod:airthermoupper-" + Variant["orientation"]);


        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack? byItemStack = null) {
            base.OnBlockPlaced(world, blockPos, byItemStack);

            var upperBlock = world.GetBlock(UpperBlockCode);

            world.BlockAccessor.SetBlock(upperBlock!.BlockId, blockPos.UpCopy());
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1) {
            var upperPos = pos.UpCopy();
            var upBlock = api.World.BlockAccessor.GetBlock(upperPos);
            if (upBlock.Code == UpperBlockCode) {
                world.BlockAccessor.SetBlock(0, upperPos);
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
            else {
                api.Logger.Warning("Couldn't find Block Entity (BEAirThermo). Have you loaded this save without mods?");
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}