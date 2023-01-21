using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Electricity.Content
{
    public class BlockAccumulator : Block
    {
        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(BlockFacing.DOWN)).SideSolid[4])
            {
                return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            }
            return false;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            base.OnNeighbourBlockChange(world, pos, neibpos);
            if (!world.BlockAccessor.GetBlock(pos.AddCopy(BlockFacing.DOWN)).SideSolid[4])
            {
                world.BlockAccessor.BreakBlock(pos, (IPlayer)null, 1f);
            }
        }
    }
}
