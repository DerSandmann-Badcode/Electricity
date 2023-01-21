using System;
using System.Text;
using Electricity.Interface;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Electricity.Content
{
    public sealed class BEBehaviorAccumulator : BlockEntityBehavior, IElectricAccumulator
    {
        private int _capacity;

        public BEBehaviorAccumulator(BlockEntity blockEntity)
            : base(blockEntity)
        {
        }

        public int GetMaxCapacity()
        {
            return 16000;
        }

        public int GetCapacity()
        {
            return _capacity;
        }

        public void Store(int amount)
        {
            _capacity += amount;
        }

        public void Release(int amount)
        {
            _capacity -= amount;
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("electricity:capacity", _capacity);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            _capacity = tree.GetInt("electricity:capacity", 0);
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder stringBuilder)
        {
            base.GetBlockInfo(forPlayer, stringBuilder);
            stringBuilder.AppendLine(GetCapacity() + "/" + GetMaxCapacity());
        }
    }
}
