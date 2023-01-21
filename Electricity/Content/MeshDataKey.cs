using Electricity.Utils;

namespace Electricity.Content
{
    internal struct MeshDataKey
    {
        public readonly Facing Connection;

        public readonly Facing Switches;

        public readonly Facing SwitchesState;

        public MeshDataKey(Facing connection, Facing switches, Facing switchesState)
        {
            Connection = connection;
            Switches = switches;
            SwitchesState = switchesState;
        }
    }
}
