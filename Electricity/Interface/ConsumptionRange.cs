namespace Electricity.Interface
{
    public struct ConsumptionRange
    {
        public readonly int Min;

        public readonly int Max;

        public ConsumptionRange(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
}
