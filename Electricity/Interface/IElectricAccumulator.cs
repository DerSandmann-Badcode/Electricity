namespace Electricity.Interface
{
    public interface IElectricAccumulator
    {
        int GetMaxCapacity();

        int GetCapacity();

        void Store(int amount);

        void Release(int amount);
    }
}
