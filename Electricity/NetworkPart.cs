using Electricity.Interface;
using Electricity.Utils;
using Vintagestory.API.MathTools;

namespace Electricity
{
    internal class NetworkPart
    {
        public readonly BlockPos Position;

        public readonly Network[] Networks = new Network[6];

        public Facing Connection;

        public IElectricConsumer Consumer;

        public IElectricProducer Producer;

        public IElectricAccumulator Accumulator;

        public NetworkPart(BlockPos position)
        {
            Position = position;
        }
    }
}
