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
    public sealed class BEBehaviorMotor : BEBehaviorMPBase, IElectricConsumer
    {
        private const float AccelerationFactor = 1f;

        private int _powerSetting;

        private float _resistance = 0.03f;

        private double _capableSpeed;

        private static CompositeShape _compositeShape;

        public override BlockFacing OutFacingForNetworkDiscovery
        {
            get
            {
                if (((BlockEntityBehavior)this).Blockentity is BlockEntityMotor blockEntityMotor && blockEntityMotor.Facing != 0)
                {
                    return FacingHelper.Directions(blockEntityMotor.Facing).First();
                }
                return BlockFacing.NORTH;
            }
        }

        private float TargetSpeed => 0.01f * (float)_powerSetting;

        private float TorqueFactor => 0.007f * (float)_powerSetting;

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

        public ConsumptionRange ConsumptionRange => new ConsumptionRange(10, 100);

        public BEBehaviorMotor(BlockEntity blockEntity)
        : base(blockEntity)
        {
        }

        public override float GetResistance()
        {
            if (_powerSetting == 0)
            {
                return 0.25f;
            }
            return FloatHelper.Remap((float)_powerSetting / 100f, 0f, 1f, 0.01f, 0.075f);
        }

        public override float GetTorque(long tick, float speed, out float resistance)
        {
            _resistance = ((BEBehaviorMPBase)this).GetResistance();
            _capableSpeed += ((double)TargetSpeed - _capableSpeed) * 1.0;
            float num = (float)_capableSpeed;
            float num2 = ((base.propagationDir == ((BEBehaviorMPBase)this).OutFacingForNetworkDiscovery) ? 1f : (-1f));
            float num3 = Math.Abs(speed);
            float num4 = num3 - num;
            bool flag = num2 * speed < 0f;
            resistance = (flag ? (_resistance * TorqueFactor * Math.Min(0.8f, num3 * 400f)) : ((num4 > 0f) ? (_resistance * Math.Min(0.2f, num4 * num4 * 80f)) : 0f));
            float val = (flag ? num : (num - num3));
            return Math.Max(0f, val) * TorqueFactor * num2;
        }

        public override void WasPlaced(BlockFacing connectedOnFacing)
        {
        }

        protected override CompositeShape GetShape()
        {
            ICoreAPI api = this.Api;
            if (api != null && ((BlockEntityBehavior)this).Blockentity is BlockEntityMotor blockEntityMotor && blockEntityMotor.Facing != 0)
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
            base.Shape = this.GetShape();
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
        {
            return false;
        }

        public void Consume(int amount)
        {
            if (_powerSetting != amount)
            {
                _powerSetting = amount;
                base.Blockentity.MarkDirty(true, (IPlayer)null);
            }
        }
    }
}
