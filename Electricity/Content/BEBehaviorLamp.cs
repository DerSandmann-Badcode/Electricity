using System;
using Electricity.Interface;
using Electricity.Utils;
using Vintagestory.API.Common;

namespace Electricity.Content
{
    public sealed class BEBehaviorLamp : BlockEntityBehavior, IElectricConsumer
    {
        private int _lightLevel;

        public ConsumptionRange ConsumptionRange => new ConsumptionRange(1, 8);

        public BEBehaviorLamp(BlockEntity blockEntity)
            : base(blockEntity)
        {
        }

        public void Consume(int lightLevel)
        {
            ICoreAPI api = base.Api;
            if (api != null && lightLevel != _lightLevel)
            {
                if (_lightLevel == 0 && lightLevel > 0)
                {
                    AssetLocation val = ((RegistryObject)base.Blockentity.Block).CodeWithVariant("state", "enabled");
                    Block block = api.World.BlockAccessor.GetBlock(val);
                    api.World.BlockAccessor.ExchangeBlock(((CollectibleObject)block).Id, base.Blockentity.Pos);
                }
                if (_lightLevel > 0 && lightLevel == 0)
                {
                    AssetLocation val2 = ((RegistryObject)base.Blockentity.Block).CodeWithVariant("state", "disabled");
                    Block block2 = api.World.BlockAccessor.GetBlock(val2);
                    api.World.BlockAccessor.ExchangeBlock(((CollectibleObject)block2).Id, base.Blockentity.Pos);
                }
                base.Blockentity.Block.LightHsv = new byte[3]
                {
                (byte)FloatHelper.Remap(lightLevel, 0f, 8f, 0f, 8f),
                (byte)FloatHelper.Remap(lightLevel, 0f, 8f, 0f, 2f),
                (byte)FloatHelper.Remap(lightLevel, 0f, 8f, 0f, 21f)
                };
                base.Blockentity.MarkDirty(true, (IPlayer)null);
                _lightLevel = lightLevel;
            }
        }
    }
}
