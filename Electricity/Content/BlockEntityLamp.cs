using Electricity.Utils;
using Vintagestory.API.Common;

namespace Electricity.Content
{
    public class BlockEntityLamp : BlockEntity
    {
        private BEBehaviorElectricity Electricity => ((BlockEntity)this).GetBehavior<BEBehaviorElectricity>();

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);
            Electricity.Connection = Facing.UpAll;
        }
    }
}
