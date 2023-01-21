using Electricity.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace Electricity.Content
{
    public class BlockEntityMotor : BlockEntity
    {
        private Facing _facing;

        private BEBehaviorElectricity Electricity => ((BlockEntity)this).GetBehavior<BEBehaviorElectricity>();

        public Facing Facing
        {
            get
            {
                return _facing;
            }
            set
            {
                if (_facing != value)
                {
                    Electricity.Connection = FacingHelper.FullFace(_facing = value);
                }
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetBytes("electricity:facing", SerializerUtil.Serialize<Facing>(_facing));
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            _facing = SerializerUtil.Deserialize<Facing>(tree.GetBytes("electricity:facing", (byte[])null));
        }
    }
}
