using System;
using Electricity.Utils;
using Vintagestory.API.Common;

namespace Electricity.Content
{
    public class BlockSwitch : Block
    {
        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            Selection selection = new Selection(blockSel);
            Facing facing = FacingHelper.FromFace(selection.Face);
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityCable blockEntityCable)
            {
                BEBehaviorElectricity behavior = ((BlockEntity)blockEntityCable).GetBehavior<BEBehaviorElectricity>();
                if (behavior != null && (blockEntityCable.Switches & facing) == 0 && (behavior.Connection & facing) != 0)
                {
                    blockEntityCable.Switches = (blockEntityCable.Switches & ~facing) | selection.Facing;
                    blockEntityCable.SwitchesState |= facing;
                    ((BlockEntity)blockEntityCable).MarkDirty(true, (IPlayer)null);
                    return true;
                }
            }
            return false;
        }
    }
}
