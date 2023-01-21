namespace Electricity.Interface
{
    public interface IElectricConsumer
    {
        ConsumptionRange ConsumptionRange { get; }

        void Consume(int amount);
    }
}
