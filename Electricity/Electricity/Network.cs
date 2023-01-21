using System.Collections.Generic;
using Electricity.Interface;
using Vintagestory.API.MathTools;

namespace Electricity
{
    internal class Network
    {
        public readonly HashSet<BlockPos> PartPositions = new HashSet<BlockPos>();

        public readonly HashSet<IElectricConsumer> Consumers = new HashSet<IElectricConsumer>();

        public readonly HashSet<IElectricProducer> Producers = new HashSet<IElectricProducer>();

        public readonly HashSet<IElectricAccumulator> Accumulators = new HashSet<IElectricAccumulator>();

        public int Production;

        public int Consumption;

        public int Overflow;
    }
}
