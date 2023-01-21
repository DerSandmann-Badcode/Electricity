using System;
using Electricity.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Electricity.Content
{
    public class BlockEntityCable : BlockEntity
    {
        private Facing _switches;

        private BEBehaviorElectricity Electricity => base.GetBehavior<BEBehaviorElectricity>();

        public Facing Connection
        {
            get
            {
                return Electricity.Connection;
            }
            set
            {
                Electricity.Connection = value;
            }
        }

        public Facing Switches
        {
            get
            {
                return _switches;
            }
            set
            {
                Electricity.Interruption &= (_switches = value);
            }
        }

        public Facing SwitchesState
        {
            get
            {
                return ~Electricity.Interruption;
            }
            set
            {
                Electricity.Interruption = ~value;
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetBytes("electricity:switches", SerializerUtil.Serialize<Facing>(_switches));
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            _switches = SerializerUtil.Deserialize<Facing>(tree.GetBytes("electricity:switches", (byte[])null));
        }
    }
}
