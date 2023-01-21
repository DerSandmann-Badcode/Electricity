using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Electricity.Content
{
    public class BlockLamp : Block
    {
        private readonly Cuboidf[] _collisionBoxes = (Cuboidf[])(object)new Cuboidf[1]
        {
        new Cuboidf(0f, 0.9375f, 0f, 1f, 1f, 1f)
        };

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            if (world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(BlockFacing.UP)).SideSolid[5])
            {
                return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
            }
            return false;
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)
            //IL_0010: Expected O, but got Unknown
            //IL_0023: Unknown result type (might be due to invalid IL or missing references)
            //IL_0029: Expected O, but got Unknown
            Block block = world.GetBlock(new AssetLocation("electricity:lamp-disabled"));
            return (ItemStack[])(object)new ItemStack[1]
            {
            new ItemStack(block, (int)Math.Ceiling(dropQuantityMultiplier))
            };
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            base.OnNeighbourBlockChange(world, pos, neibpos);
            if (!world.BlockAccessor.GetBlock(pos.AddCopy(BlockFacing.UP)).SideSolid[5])
            {
                world.BlockAccessor.BreakBlock(pos, (IPlayer)null, 1f);
            }
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return _collisionBoxes;
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return _collisionBoxes;
        }

        public override Cuboidf[] GetParticleCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return _collisionBoxes;
        }
    }
}
