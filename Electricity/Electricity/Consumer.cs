using Electricity.Interface;

namespace Electricity
{
    internal class Consumer
    {
        public readonly IElectricConsumer ElectricConsumer;

        public readonly ConsumptionRange Consumption;

        public int GivenEnergy;

        public Consumer(IElectricConsumer electricConsumer)
        {
            ElectricConsumer = electricConsumer;
            Consumption = electricConsumer.ConsumptionRange;
        }
    }
}
