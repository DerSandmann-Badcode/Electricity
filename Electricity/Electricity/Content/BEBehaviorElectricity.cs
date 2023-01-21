using System;
using System.Text;
using Electricity.Interface;
using Electricity.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Electricity.Content
{
    public sealed class BEBehaviorElectricity : BlockEntityBehavior
    {
        private Facing _connection;

        private Facing _interruption;

        private bool _dirty = true;

        private IElectricConsumer _consumer;

        private IElectricProducer _producer;

        private IElectricAccumulator _accumulator;

        public Facing Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                if (_connection != value)
                {
                    _connection = value;
                    Update();
                }
            }
        }

        public Facing Interruption
        {
            get
            {
                return _interruption;
            }
            set
            {
                if (_interruption != value)
                {
                    _interruption = value;
                    Update();
                }
            }
        }

        private void Update()
        {
            _dirty = true;
            ICoreAPI api = base.Api;
            if (api == null)
            {
                return;
            }
            Electricity modSystem = api.ModLoader.GetModSystem<Electricity>(true);
            if (modSystem == null)
            {
                return;
            }
            _dirty = false;
            _consumer = null;
            _producer = null;
            _accumulator = null;
            foreach (BlockEntityBehavior behavior in base.Blockentity.Behaviors)
            {
                if (!(behavior is IElectricConsumer consumer))
                {
                    if (!(behavior is IElectricProducer producer))
                    {
                        if (behavior is IElectricAccumulator accumulator)
                        {
                            _accumulator = accumulator;
                        }
                    }
                    else
                    {
                        _producer = producer;
                    }
                }
                else
                {
                    _consumer = consumer;
                }
            }
            modSystem.Update(base.Blockentity.Pos, _connection & ~_interruption);
            modSystem.SetConsumer(base.Blockentity.Pos, _consumer);
            modSystem.SetProducer(base.Blockentity.Pos, _producer);
            modSystem.SetAccumulator(base.Blockentity.Pos, _accumulator);
            base.Blockentity.MarkDirty(true, (IPlayer)null);
        }

        public BEBehaviorElectricity(BlockEntity blockEntity)
            : base(blockEntity)
        {
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder stringBuilder)
        {
            base.GetBlockInfo(forPlayer, stringBuilder);
            NetworkInformation networks = base.Api.ModLoader.GetModSystem<Electricity>(true).GetNetworks(base.Blockentity.Pos, Connection);
            stringBuilder.AppendLine("Electricity");
            stringBuilder.AppendLine("├ Production: " + networks.Production + "⚡\u2003\u2003\u2003");
            stringBuilder.AppendLine("├ Consumption: " + networks.Consumption + "⚡\u2003\u2003\u2003");
            stringBuilder.AppendLine("└ Overflow: " + networks.Overflow + "⚡\u2003\u2003\u2003");
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            if (_dirty)
            {
                Update();
            }
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            ICoreAPI api = base.Api;
            if (api != null)
            {
                api.ModLoader.GetModSystem<Electricity>(true).Remove(base.Blockentity.Pos);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetBytes("electricity:connection", SerializerUtil.Serialize<Facing>(_connection));
            tree.SetBytes("electricity:interruption", SerializerUtil.Serialize<Facing>(_interruption));
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            _connection = SerializerUtil.Deserialize<Facing>(tree.GetBytes("electricity:connection", (byte[])null));
            _interruption = SerializerUtil.Deserialize<Facing>(tree.GetBytes("electricity:interruption", (byte[])null));
            Update();
        }
    }
}
