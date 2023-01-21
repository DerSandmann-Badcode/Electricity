using System;
using System.Collections.Generic;
using System.Linq;
using Electricity.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace Electricity.Content
{
    public class BlockMotor : Block, IMechanicalPowerBlock
    {
        private static readonly Dictionary<Facing, MeshData> MeshData = new Dictionary<Facing, MeshData>();

        public override void OnLoaded(ICoreAPI coreApi)
        {
            base.OnLoaded(coreApi);
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            Selection selection = new Selection(blockSel);
            BlockFacing val = FacingHelper.Faces(FacingHelper.From(selection.Face, selection.Direction)).First();
            if (val != null && !world.BlockAccessor.GetBlock(blockSel.Position.AddCopy(val)).SideSolid[val.Opposite.Index])
            {
                return false;
            }
            return base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode);
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            Selection selection = new Selection(blockSel);
            Facing facing = FacingHelper.From(selection.Face, selection.Direction);
            if (base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack) && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityMotor blockEntityMotor)
            {
                blockEntityMotor.Facing = facing;
                BlockFacing val = FacingHelper.Directions(blockEntityMotor.Facing).First();
                BlockPos position = blockSel.Position;
                BlockPos val2 = position.AddCopy(val);
                Block block = world.BlockAccessor.GetBlock(val2);
                IMechanicalPowerBlock val3 = (IMechanicalPowerBlock)(object)((block is IMechanicalPowerBlock) ? block : null);
                if (val3 != null && val3.HasMechPowerConnectorAt(world, val2, val.Opposite))
                {
                    val3.DidConnectAt(world, val2, val.Opposite);
                    BlockEntity blockEntity = world.BlockAccessor.GetBlockEntity(position);
                    if (blockEntity != null)
                    {
                        BEBehaviorMPBase behavior = blockEntity.GetBehavior<BEBehaviorMPBase>();
                        if (behavior != null)
                        {
                            behavior.tryConnect(val);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            base.OnNeighbourBlockChange(world, pos, neibpos);
            if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityMotor blockEntityMotor)
            {
                BlockFacing val = FacingHelper.Faces(blockEntityMotor.Facing).First();
                if (val != null && !world.BlockAccessor.GetBlock(pos.AddCopy(val)).SideSolid[val.Opposite.Index])
                {
                    world.BlockAccessor.BreakBlock(pos, (IPlayer)null, 1f);
                }
            }
        }

        public MechanicalNetwork GetNetwork(IWorldAccessor world, BlockPos pos)
        {
            BlockEntity blockEntity = world.BlockAccessor.GetBlockEntity(pos);
            IMechanicalPowerDevice val = (IMechanicalPowerDevice)(object)((blockEntity != null) ? blockEntity.GetBehavior<BEBehaviorMPBase>() : null);
            if (val != null)
            {
                return val.Network;
            }
            return null;
        }

        public bool HasMechPowerConnectorAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        {
            if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityMotor blockEntityMotor && blockEntityMotor.Facing != 0)
            {
                return FacingHelper.Directions(blockEntityMotor.Facing).First() == face;
            }
            return false;
        }

        public void DidConnectAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        {
        }

        public override void OnJsonTesselation(ref MeshData sourceMesh, ref int[] lightRgbsByCorner, BlockPos pos, Block[] chunkExtBlocks, int extIndex3d)
        {
            base.OnJsonTesselation(ref sourceMesh, ref lightRgbsByCorner, pos, chunkExtBlocks, extIndex3d);
            ICoreAPI api = this.api;
            ICoreClientAPI val = (ICoreClientAPI)(object)((api is ICoreClientAPI) ? api : null);
            if (val == null || !(this.api.World.BlockAccessor.GetBlockEntity(pos) is BlockEntityMotor blockEntityMotor) || blockEntityMotor.Facing == Facing.None)
            {
                return;
            }
            Facing facing = blockEntityMotor.Facing;
            if (!MeshData.TryGetValue(facing, out var value))
            {
                Vec3f val2 = new Vec3f(0.5f, 0.5f, 0.5f);
                Block block = val.World.GetBlock(new AssetLocation("electricity:motor-stator"));
                val.Tesselator.TesselateBlock(block, out value);
                if ((facing & Facing.NorthEast) != 0)
                {
                    value.Rotate(val2, (float)Math.PI / 2f, 4.712389f, 0f);
                }
                if ((facing & Facing.NorthWest) != 0)
                {
                    value.Rotate(val2, (float)Math.PI / 2f, (float)Math.PI / 2f, 0f);
                }
                if ((facing & Facing.NorthUp) != 0)
                {
                    value.Rotate(val2, (float)Math.PI / 2f, 0f, 0f);
                }
                if ((facing & Facing.NorthDown) != 0)
                {
                    value.Rotate(val2, (float)Math.PI / 2f, (float)Math.PI, 0f);
                }
                if ((facing & Facing.EastNorth) != 0)
                {
                    value.Rotate(val2, 0f, 0f, (float)Math.PI / 2f);
                }
                if ((facing & Facing.EastSouth) != 0)
                {
                    value.Rotate(val2, (float)Math.PI, 0f, (float)Math.PI / 2f);
                }
                if ((facing & Facing.EastUp) != 0)
                {
                    value.Rotate(val2, (float)Math.PI / 2f, 0f, (float)Math.PI / 2f);
                }
                if ((facing & Facing.EastDown) != 0)
                {
                    value.Rotate(val2, 4.712389f, 0f, (float)Math.PI / 2f);
                }
                if ((facing & Facing.SouthEast) != 0)
                {
                    value.Rotate(val2, (float)Math.PI / 2f, 4.712389f, (float)Math.PI);
                }
                if ((facing & Facing.SouthWest) != 0)
                {
                    value.Rotate(val2, (float)Math.PI / 2f, (float)Math.PI / 2f, (float)Math.PI);
                }
                if ((facing & Facing.SouthUp) != 0)
                {
                    value.Rotate(val2, (float)Math.PI / 2f, 0f, (float)Math.PI);
                }
                if ((facing & Facing.SouthDown) != 0)
                {
                    value.Rotate(val2, (float)Math.PI / 2f, (float)Math.PI, (float)Math.PI);
                }
                if ((facing & Facing.WestNorth) != 0)
                {
                    value.Rotate(val2, 0f, 0f, 4.712389f);
                }
                if ((facing & Facing.WestSouth) != 0)
                {
                    value.Rotate(val2, (float)Math.PI, 0f, 4.712389f);
                }
                if ((facing & Facing.WestUp) != 0)
                {
                    value.Rotate(val2, (float)Math.PI / 2f, 0f, 4.712389f);
                }
                if ((facing & Facing.WestDown) != 0)
                {
                    value.Rotate(val2, 4.712389f, 0f, 4.712389f);
                }
                if ((facing & Facing.UpNorth) != 0)
                {
                    value.Rotate(val2, 0f, 0f, (float)Math.PI);
                }
                if ((facing & Facing.UpEast) != 0)
                {
                    value.Rotate(val2, 0f, 4.712389f, (float)Math.PI);
                }
                if ((facing & Facing.UpSouth) != 0)
                {
                    value.Rotate(val2, 0f, (float)Math.PI, (float)Math.PI);
                }
                if ((facing & Facing.UpWest) != 0)
                {
                    value.Rotate(val2, 0f, (float)Math.PI / 2f, (float)Math.PI);
                }
                if ((facing & Facing.DownNorth) != 0)
                {
                    value.Rotate(val2, 0f, 0f, 0f);
                }
                if ((facing & Facing.DownEast) != 0)
                {
                    value.Rotate(val2, 0f, 4.712389f, 0f);
                }
                if ((facing & Facing.DownSouth) != 0)
                {
                    value.Rotate(val2, 0f, (float)Math.PI, 0f);
                }
                if ((facing & Facing.DownWest) != 0)
                {
                    value.Rotate(val2, 0f, (float)Math.PI / 2f, 0f);
                }
                MeshData.Add(facing, value);
            }
            sourceMesh = value;
        }
    }
}
