using System;
using System.Collections.Generic;
using System.Linq;
using Electricity.Content;
using Electricity.Interface;
using Electricity.Utils;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Electricity
{
    public class Electricity : ModSystem
    {
        private readonly Dictionary<BlockPos, NetworkPart> _parts = new Dictionary<BlockPos, NetworkPart>();

        private readonly HashSet<Network> _networks = new HashSet<Network>();

        private readonly List<Consumer> _consumers = new List<Consumer>();

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("Accumulator", typeof(BlockAccumulator));
            api.RegisterBlockEntityClass("Accumulator", typeof(BlockEntityAccumulator));
            api.RegisterBlockEntityBehaviorClass("Accumulator", typeof(BEBehaviorAccumulator));
            api.RegisterBlockClass("Cable", typeof(BlockCable));
            api.RegisterBlockEntityClass("Cable", typeof(BlockEntityCable));
            api.RegisterBlockClass("Motor", typeof(BlockMotor));
            api.RegisterBlockEntityClass("Motor", typeof(BlockEntityMotor));
            api.RegisterBlockEntityBehaviorClass("Motor", typeof(BEBehaviorMotor));
            api.RegisterBlockClass("Generator", typeof(BlockGenerator));
            api.RegisterBlockEntityClass("Generator", typeof(BlockEntityGenerator));
            api.RegisterBlockEntityBehaviorClass("Generator", typeof(BEBehaviorGenerator));
            api.RegisterBlockClass("Lamp", typeof(BlockLamp));
            api.RegisterBlockEntityClass("Lamp", typeof(BlockEntityLamp));
            api.RegisterBlockEntityBehaviorClass("Lamp", typeof(BEBehaviorLamp));
            api.RegisterBlockClass("Switch", typeof(BlockSwitch));
            api.RegisterBlockEntityBehaviorClass("Electricity", typeof(BEBehaviorElectricity));
            api.Event.RegisterGameTickListener(OnGameTick, 500, 0);
        }

        private void OnGameTick(float _)
        {
            List<IElectricAccumulator> list = new List<IElectricAccumulator>();
            foreach (Network network in _networks)
            {
                _consumers.Clear();
                int num = 0;
                foreach (IElectricProducer producer in network.Producers)
                {
                    num += producer.Produce();
                }
                int num2 = 0;
                foreach (IElectricConsumer consumer4 in network.Consumers)
                {
                    Consumer consumer2 = new Consumer(consumer4);
                    num2 += consumer2.Consumption.Max;
                    _consumers.Add(consumer2);
                }
                if (num < num2)
                {
                    do
                    {
                        list.Clear();
                        foreach (IElectricAccumulator accumulator in network.Accumulators)
                        {
                            if (accumulator.GetCapacity() > 0)
                            {
                                list.Add(accumulator);
                            }
                        }
                        if (list.Count <= 0)
                        {
                            continue;
                        }
                        int num3 = (num2 - num) / list.Count;
                        if (num3 == 0)
                        {
                            break;
                        }
                        foreach (IElectricAccumulator item in list)
                        {
                            int num4 = Math.Min(item.GetCapacity(), num3);
                            if (num4 > 0)
                            {
                                num += num4;
                                item.Release(num4);
                            }
                        }
                    }
                    while (list.Count > 0 && num2 - num > 0);
                }
                int availableEnergy = num;
                Consumer[] source = (from consumer in _consumers
                                     orderby consumer.Consumption.Min
                                     group consumer by consumer.Consumption.Min).Where(delegate (IGrouping<int, Consumer> grouping)
                                     {
                                         ConsumptionRange consumption = grouping.First().Consumption;
                                         int num10 = consumption.Min * grouping.Count();
                                         if (num10 <= availableEnergy)
                                         {
                                             availableEnergy -= num10;
                                             foreach (Consumer item2 in grouping)
                                             {
                                                 item2.GivenEnergy += consumption.Min;
                                             }
                                             return true;
                                         }
                                         return false;
                                     }).SelectMany((IGrouping<int, Consumer> grouping) => grouping).ToArray();
                int num5 = int.MaxValue;
                while (availableEnergy > 0 && num5 != 0)
                {
                    num5 = 0;
                    Consumer[] array = source.Where((Consumer consumer) => consumer.Consumption.Max > consumer.GivenEnergy).ToArray();
                    int num6 = array.Count();
                    if (num6 == 0)
                    {
                        break;
                    }
                    int val = Math.Max(1, availableEnergy / num6);
                    Consumer[] array2 = array;
                    foreach (Consumer consumer3 in array2)
                    {
                        if (availableEnergy == 0)
                        {
                            break;
                        }
                        int num7 = Math.Min(val, consumer3.Consumption.Max - consumer3.GivenEnergy);
                        availableEnergy -= num7;
                        consumer3.GivenEnergy += num7;
                        num5 += consumer3.Consumption.Max - consumer3.GivenEnergy;
                    }
                }
                foreach (Consumer consumer5 in _consumers)
                {
                    consumer5.ElectricConsumer.Consume(consumer5.GivenEnergy);
                }
                network.Production = num;
                network.Consumption = num - availableEnergy;
                while ((network.Overflow = network.Production - network.Consumption) > 0)
                {
                    list.Clear();
                    foreach (IElectricAccumulator accumulator2 in network.Accumulators)
                    {
                        if (accumulator2.GetMaxCapacity() - accumulator2.GetCapacity() > 0)
                        {
                            list.Add(accumulator2);
                        }
                    }
                    if (list.Count == 0)
                    {
                        break;
                    }
                    int num8 = network.Overflow / list.Count;
                    if (num8 == 0)
                    {
                        break;
                    }
                    foreach (IElectricAccumulator item3 in list)
                    {
                        int num9 = Math.Min(num8, item3.GetMaxCapacity() - item3.GetCapacity());
                        item3.Store(num9);
                        network.Consumption += num9;
                    }
                }
            }
        }

        private Network MergeNetworks(HashSet<Network> networks)
        {
            Network network = null;
            foreach (Network network2 in networks)
            {
                if (network == null || network.PartPositions.Count < network2.PartPositions.Count)
                {
                    network = network2;
                }
            }
            if (network != null)
            {
                foreach (Network network3 in networks)
                {
                    if (network == network3)
                    {
                        continue;
                    }
                    foreach (BlockPos partPosition in network3.PartPositions)
                    {
                        NetworkPart networkPart = _parts[partPosition];
                        BlockFacing[] aLLFACES = BlockFacing.ALLFACES;
                        foreach (BlockFacing val in aLLFACES)
                        {
                            if (networkPart.Networks[val.Index] == network3)
                            {
                                networkPart.Networks[val.Index] = network;
                            }
                        }
                        IElectricConsumer consumer = networkPart.Consumer;
                        if (consumer != null)
                        {
                            network.Consumers.Add(consumer);
                        }
                        IElectricProducer producer = networkPart.Producer;
                        if (producer != null)
                        {
                            network.Producers.Add(producer);
                        }
                        IElectricAccumulator accumulator = networkPart.Accumulator;
                        if (accumulator != null)
                        {
                            network.Accumulators.Add(accumulator);
                        }
                        network.PartPositions.Add(partPosition);
                    }
                    network3.PartPositions.Clear();
                    _networks.Remove(network3);
                }
            }
            return network ?? CreateNetwork();
        }

        private void RemoveNetwork(ref Network network)
        {
            BlockPos[] array = (BlockPos[])(object)new BlockPos[network.PartPositions.Count];
            network.PartPositions.CopyTo(array);
            _networks.Remove(network);
            BlockPos[] array2 = array;
            foreach (BlockPos key in array2)
            {
                if (!_parts.TryGetValue(key, out var value))
                {
                    continue;
                }
                BlockFacing[] aLLFACES = BlockFacing.ALLFACES;
                foreach (BlockFacing val in aLLFACES)
                {
                    if (value.Networks[val.Index] == network)
                    {
                        value.Networks[val.Index] = null;
                    }
                }
            }
            array2 = array;
            foreach (BlockPos key2 in array2)
            {
                if (_parts.TryGetValue(key2, out var value2))
                {
                    AddConnections(ref value2, value2.Connection);
                }
            }
        }

        private Network CreateNetwork()
        {
            Network network = new Network();
            _networks.Add(network);
            return network;
        }

        private void AddConnections(ref NetworkPart part, Facing addedConnections)
        {
            if (addedConnections == Facing.None)
            {
                return;
            }
            HashSet<Network>[] array = new HashSet<Network>[6]
            {
            new HashSet<Network>(),
            new HashSet<Network>(),
            new HashSet<Network>(),
            new HashSet<Network>(),
            new HashSet<Network>(),
            new HashSet<Network>()
            };
            foreach (BlockFacing item in FacingHelper.Faces(part.Connection))
            {
                array[item.Index].Add(part.Networks[item.Index] ?? CreateNetwork());
            }
            foreach (BlockFacing item2 in FacingHelper.Directions(addedConnections))
            {
                Facing facing = FacingHelper.FromDirection(item2);
                BlockPos key = part.Position.AddCopy(item2);
                if (!_parts.TryGetValue(key, out var value))
                {
                    continue;
                }
                foreach (BlockFacing item3 in FacingHelper.Faces(addedConnections & facing))
                {
                    if ((value.Connection & FacingHelper.From(item3, item2.Opposite)) != 0)
                    {
                        Network network = value.Networks[item3.Index];
                        if (network != null)
                        {
                            array[item3.Index].Add(network);
                        }
                    }
                    if ((value.Connection & FacingHelper.From(item2.Opposite, item3)) != 0)
                    {
                        Network network2 = value.Networks[item2.Opposite.Index];
                        if (network2 != null)
                        {
                            array[item3.Index].Add(network2);
                        }
                    }
                }
            }
            foreach (BlockFacing item4 in FacingHelper.Directions(addedConnections))
            {
                Facing facing2 = FacingHelper.FromDirection(item4);
                foreach (BlockFacing item5 in FacingHelper.Faces(addedConnections & facing2))
                {
                    BlockPos key2 = part.Position.AddCopy(item4).AddCopy(item5);
                    if (!_parts.TryGetValue(key2, out var value2))
                    {
                        continue;
                    }
                    if ((value2.Connection & FacingHelper.From(item4.Opposite, item5.Opposite)) != 0)
                    {
                        Network network3 = value2.Networks[item4.Opposite.Index];
                        if (network3 != null)
                        {
                            array[item5.Index].Add(network3);
                        }
                    }
                    if ((value2.Connection & FacingHelper.From(item5.Opposite, item4.Opposite)) != 0)
                    {
                        Network network4 = value2.Networks[item5.Opposite.Index];
                        if (network4 != null)
                        {
                            array[item5.Index].Add(network4);
                        }
                    }
                }
            }
            foreach (BlockFacing item6 in FacingHelper.Faces(part.Connection))
            {
                Network network5 = MergeNetworks(array[item6.Index]);
                IElectricConsumer consumer = part.Consumer;
                if (consumer != null)
                {
                    network5.Consumers.Add(consumer);
                }
                IElectricProducer producer = part.Producer;
                if (producer != null)
                {
                    network5.Producers.Add(producer);
                }
                IElectricAccumulator accumulator = part.Accumulator;
                if (accumulator != null)
                {
                    network5.Accumulators.Add(accumulator);
                }
                network5.PartPositions.Add(part.Position);
                part.Networks[item6.Index] = network5;
            }
            foreach (BlockFacing item7 in FacingHelper.Directions(part.Connection))
            {
                Facing facing3 = FacingHelper.FromDirection(item7);
                foreach (BlockFacing item8 in FacingHelper.Faces(part.Connection & facing3))
                {
                    if ((part.Connection & FacingHelper.From(item7, item8)) == 0)
                    {
                        continue;
                    }
                    Network network6 = part.Networks[item8.Index];
                    if (network6 != null)
                    {
                        Network network7 = part.Networks[item7.Index];
                        if (network7 != null)
                        {
                            HashSet<Network> hashSet = new HashSet<Network>();
                            hashSet.Add(network6);
                            hashSet.Add(network7);
                            MergeNetworks(hashSet);
                            continue;
                        }
                    }
                    throw new Exception();
                }
            }
        }

        private void RemoveConnections(ref NetworkPart part, Facing removedConnections)
        {
            if (removedConnections == Facing.None)
            {
                return;
            }
            foreach (BlockFacing item in FacingHelper.Faces(removedConnections))
            {
                Network network = part.Networks[item.Index];
                if (network != null)
                {
                    RemoveNetwork(ref network);
                }
            }
        }

        public bool Update(BlockPos position, Facing facing)
        {
            if (!_parts.TryGetValue(position, out var value))
            {
                if (facing == Facing.None)
                {
                    return false;
                }
                NetworkPart networkPart2 = (_parts[position] = new NetworkPart(position));
                value = networkPart2;
            }
            if (facing == value.Connection)
            {
                return false;
            }
            Facing addedConnections = ~value.Connection & facing;
            Facing removedConnections = value.Connection & ~facing;
            value.Connection = facing;
            AddConnections(ref value, addedConnections);
            RemoveConnections(ref value, removedConnections);
            if (value.Connection == Facing.None)
            {
                _parts.Remove(position);
            }
            return true;
        }

        public void Remove(BlockPos position)
        {
            if (_parts.TryGetValue(position, out var value))
            {
                _parts.Remove(position);
                RemoveConnections(ref value, value.Connection);
            }
        }

        public void SetConsumer(BlockPos position, IElectricConsumer consumer)
        {
            if (!_parts.TryGetValue(position, out var value))
            {
                if (consumer == null)
                {
                    return;
                }
                NetworkPart networkPart2 = (_parts[position] = new NetworkPart(position));
                value = networkPart2;
            }
            if (value.Consumer == consumer)
            {
                return;
            }
            Network[] networks = value.Networks;
            foreach (Network network in networks)
            {
                if (value.Consumer != null)
                {
                    network?.Consumers.Remove(value.Consumer);
                }
                if (consumer != null)
                {
                    network?.Consumers.Add(consumer);
                }
            }
            value.Consumer = consumer;
        }

        public void SetProducer(BlockPos position, IElectricProducer producer)
        {
            if (!_parts.TryGetValue(position, out var value))
            {
                if (producer == null)
                {
                    return;
                }
                NetworkPart networkPart2 = (_parts[position] = new NetworkPart(position));
                value = networkPart2;
            }
            if (value.Producer == producer)
            {
                return;
            }
            Network[] networks = value.Networks;
            foreach (Network network in networks)
            {
                if (value.Producer != null)
                {
                    network?.Producers.Remove(value.Producer);
                }
                if (producer != null)
                {
                    network?.Producers.Add(producer);
                }
            }
            value.Producer = producer;
        }

        public void SetAccumulator(BlockPos position, IElectricAccumulator accumulator)
        {
            if (!_parts.TryGetValue(position, out var value))
            {
                if (accumulator == null)
                {
                    return;
                }
                NetworkPart networkPart2 = (_parts[position] = new NetworkPart(position));
                value = networkPart2;
            }
            if (value.Accumulator == accumulator)
            {
                return;
            }
            Network[] networks = value.Networks;
            foreach (Network network in networks)
            {
                if (value.Accumulator != null)
                {
                    network?.Accumulators.Remove(value.Accumulator);
                }
                if (accumulator != null)
                {
                    network?.Accumulators.Add(accumulator);
                }
            }
            value.Accumulator = accumulator;
        }

        public NetworkInformation GetNetworks(BlockPos position, Facing facing)
        {
            NetworkInformation networkInformation = new NetworkInformation();
            if (_parts.TryGetValue(position, out var value))
            {
                foreach (BlockFacing item in FacingHelper.Faces(facing))
                {
                    Network network = value.Networks[item.Index];
                    if (network != null)
                    {
                        networkInformation.Facing |= FacingHelper.FromFace(item);
                        networkInformation.NumberOfBlocks += network.PartPositions.Count;
                        networkInformation.NumberOfConsumers += network.Consumers.Count;
                        networkInformation.NumberOfProducers += network.Producers.Count;
                        networkInformation.NumberOfAccumulators += network.Accumulators.Count;
                        networkInformation.Production += network.Production;
                        networkInformation.Consumption += network.Consumption;
                        networkInformation.Overflow += network.Overflow;
                    }
                }
                return networkInformation;
            }
            return networkInformation;
        }
    }
}
