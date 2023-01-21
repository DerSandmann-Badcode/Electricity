using System;
using System.Linq;
using Electricity.Interface;
using Electricity.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace Electricity.Content
{
    public sealed class BEBehaviorGenerator : BEBehaviorMPBase, IElectricProducer
    {
        private int _powerSetting;

        private static CompositeShape _compositeShape;

        public override BlockFacing OutFacingForNetworkDiscovery
        {
            get
            {
                if (((BlockEntityBehavior)this).Blockentity is BlockEntityGenerator blockEntityGenerator && blockEntityGenerator.Facing != 0)
                {
                    return FacingHelper.Directions(blockEntityGenerator.Facing).First();
                }
                return BlockFacing.NORTH;
            }
        }

        public override int[] AxisSign
        {
            get
            {
                switch (this.OutFacingForNetworkDiscovery.Index)
                {
                    case 0:
                        return new int[3] { 0, 0, -1 };
                    case 1:
                        return new int[3] { -1, 0, 0 };
                    case 2:
                        return new int[3] { 0, 0, -1 };
                    case 3:
                        return new int[3] { -1, 0, 0 };
                    case 4:
                        return new int[3] { 0, 1, 0 };
                    case 5:
                        return new int[3] { 0, -1, 0 };
                    default:
                        return base.AxisSign;
                }
            }
        }

        public BEBehaviorGenerator(BlockEntity blockEntity)
        : base(blockEntity)
        {
        }

        public override float GetResistance()
        {
            if (_powerSetting == 0)
            {
                return 0.05f;
            }
            return FloatHelper.Remap((float)_powerSetting / 100f, 0f, 1f, 0.01f, 0.075f);
        }

        public override void WasPlaced(BlockFacing connectedOnFacing)
        {
        }

        protected override CompositeShape GetShape()
        {
            ICoreAPI api = ((BlockEntityBehavior)this).Api;
            if (api != null && ((BlockEntityBehavior)this).Blockentity is BlockEntityGenerator blockEntityGenerator && blockEntityGenerator.Facing != 0)
            {
                BlockFacing outFacingForNetworkDiscovery = ((BEBehaviorMPBase)this).OutFacingForNetworkDiscovery;
                if (_compositeShape == null)
                {
                    AssetLocation val = ((RegistryObject)((BEBehaviorMPBase)this).Block).CodeWithVariant("type", "rotor");
                    _compositeShape = api.World.BlockAccessor.GetBlock(val)
                        .Shape.Clone();
                }
                CompositeShape val2 = _compositeShape.Clone();
                if (outFacingForNetworkDiscovery == BlockFacing.NORTH)
                {
                    val2.rotateY = 0f;
                }
                if (outFacingForNetworkDiscovery == BlockFacing.EAST)
                {
                    val2.rotateY = 270f;
                }
                if (outFacingForNetworkDiscovery == BlockFacing.SOUTH)
                {
                    val2.rotateY = 180f;
                }
                if (outFacingForNetworkDiscovery == BlockFacing.WEST)
                {
                    val2.rotateY = 90f;
                }
                if (outFacingForNetworkDiscovery == BlockFacing.UP)
                {
                    val2.rotateX = 90f;
                }
                if (outFacingForNetworkDiscovery == BlockFacing.DOWN)
                {
                    val2.rotateX = 270f;
                }
                return val2;
            }
            return null;
        }

        protected override void updateShape(IWorldAccessor worldForResolve)
        {
            ((BEBehaviorMPBase)this).Shape = this.GetShape();
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            return false;
        }

        public int Produce()
        {
            MechanicalNetwork network = base.network;
            float num = GameMath.Clamp(Math.Abs((network != null) ? network.Speed : 0f), 0f, 1f);
            int num2 = (int)(num * 100f);
            if (num2 != _powerSetting)
            {
                _powerSetting = num2;
                ((BlockEntityBehavior)this).Blockentity.MarkDirty(true, (IPlayer)null);
            }
            return (int)(num * 100f);
        }
    }
}
