using Electricity.Utils;
using Vintagestory.API.Common;

namespace Electricity.Content
{
    public class BlockEntityAccumulator : BlockEntity
    {
        private BEBehaviorElectricity Electricity => base.GetBehavior<BEBehaviorElectricity>();

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);
            Electricity.Connection = Facing.DownAll;
        }
    }
}
