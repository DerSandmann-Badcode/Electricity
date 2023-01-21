using System;
using System.Collections.Generic;
using System.Linq;
using Electricity.Utils;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Electricity.Content
{
    public class BlockCable : Block
    {
        private static readonly Dictionary<MeshDataKey, Dictionary<Facing, Cuboidf[]>> CollisionBoxesCache = new Dictionary<MeshDataKey, Dictionary<Facing, Cuboidf[]>>();

        private static readonly Dictionary<MeshDataKey, Dictionary<Facing, Cuboidf[]>> SelectionBoxesCache = new Dictionary<MeshDataKey, Dictionary<Facing, Cuboidf[]>>();

        private static readonly Dictionary<MeshDataKey, MeshData> MeshDataCache = new Dictionary<MeshDataKey, MeshData>();

        private BlockVariant _dotVariant;

        private BlockVariant _partVariant;

        private BlockVariant _enabledSwitchVariant;

        private BlockVariant _disabledSwitchVariant;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            _dotVariant = new BlockVariant(api, this, "dot");
            _partVariant = new BlockVariant(api, this, "part");
            AssetLocation val = new AssetLocation("electricity:switch-enabled");
            Block block = api.World.BlockAccessor.GetBlock(val);
            _enabledSwitchVariant = new BlockVariant(api, block, "enabled");
            _disabledSwitchVariant = new BlockVariant(api, block, "disabled");
        }

        public override bool IsReplacableBy(Block block)
        {
            if (!base.IsReplacableBy(block) && !(block is BlockCable))
            {
                return block is BlockSwitch;
            }
            return true;
        }

        public override bool DoPlaceBlock(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ItemStack byItemStack)
        {
            Selection selection = new Selection(blockSel);
            Facing facing = FacingHelper.From(selection.Face, selection.Direction);
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityCable blockEntityCable)
            {
                if ((blockEntityCable.Connection & facing) != 0)
                {
                    return false;
                }
                blockEntityCable.Connection |= facing;
                return true;
            }
            if (base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack) && world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityCable blockEntityCable2)
            {
                blockEntityCable2.Connection = facing;
                return true;
            }
            return false;
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos position, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            if (world.BlockAccessor.GetBlockEntity(position) is BlockEntityCable blockEntityCable)
            {
                AssetLocation val = new AssetLocation("electricity:cable-part");
                Block block = world.BlockAccessor.GetBlock(val);
                int num = FacingHelper.Count(blockEntityCable.Connection);
                ItemStack val2 = new ItemStack(block, num);
                return (ItemStack[])(object)new ItemStack[1] { val2 };
            }
            return base.GetDrops(world, position, byPlayer, dropQuantityMultiplier);
        }

        private bool Break(IWorldAccessor world, BlockPos position, Vec3d hitPosition, IPlayer byPlayer)
        {
            if (world.BlockAccessor.GetBlockEntity(position) is BlockEntityCable blockEntityCable)
            {
                Facing facing = Facing.None;
                Cuboidf[] array = _dotVariant?.SelectionBoxes;
                if (array != null)
                {
                    Cuboidf[] array2 = _partVariant?.SelectionBoxes;
                    if (array2 != null)
                    {
                        Cuboidf[] array3 = _enabledSwitchVariant?.SelectionBoxes;
                        if (array3 != null)
                        {
                            Cuboidf[] array4 = _disabledSwitchVariant?.SelectionBoxes;
                            if (array4 != null)
                            {
                                foreach (KeyValuePair<Facing, Cuboidf[]> item in CalculateBoxes(key: new MeshDataKey(blockEntityCable.Connection, blockEntityCable.Switches, blockEntityCable.SwitchesState), boxesCache: SelectionBoxesCache, dotBoxes: array, partBoxes: array2, enabledSwitchBoxes: array3, disabledSwitchBoxes: array4))
                                {
                                    Facing key2 = item.Key;
                                    Cuboidf[] value = item.Value;
                                    for (int i = 0; i < value.Length; i++)
                                    {
                                        if (value[i].Clone().OmniGrowBy(0.005f).Contains(hitPosition.X, hitPosition.Y, hitPosition.Z))
                                        {
                                            facing |= key2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                Facing facing2 = blockEntityCable.Switches & facing;
                if (facing2 != 0)
                {
                    blockEntityCable.Switches &= ~facing;
                    int num = FacingHelper.Faces(facing2).Count();
                    if (num > 0)
                    {
                        if ((int)byPlayer.WorldData.CurrentGameMode != 2)
                        {
                            AssetLocation val = new AssetLocation("electricity:switch-enabled");
                            ItemStack val2 = new ItemStack(world.BlockAccessor.GetBlock(val), num);
                            world.SpawnItemEntity(val2, position.ToVec3d(), (Vec3d)null);
                        }
                        return true;
                    }
                }
                Facing facing3 = blockEntityCable.Connection & ~facing;
                if (facing3 != 0)
                {
                    int num2 = FacingHelper.Count(facing);
                    if (num2 > 0)
                    {
                        if ((int)byPlayer.WorldData.CurrentGameMode != 2)
                        {
                            AssetLocation val3 = new AssetLocation("electricity:cable-part");
                            ItemStack val4 = new ItemStack(world.BlockAccessor.GetBlock(val3), num2);
                            world.SpawnItemEntity(val4, position.ToVec3d(), (Vec3d)null);
                        }
                        blockEntityCable.Connection = facing3;
                        return true;
                    }
                }
            }
            return false;
        }

        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1f)
        {
            if (byPlayer != null)
            {
                BlockSelection currentBlockSelection = byPlayer.CurrentBlockSelection;
                if (currentBlockSelection != null)
                {
                    Vec3d hitPosition = currentBlockSelection.HitPosition;
                    if (Break(world, pos, hitPosition, byPlayer))
                    {
                        return;
                    }
                }
            }
            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }

        public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos)
        {
            base.OnNeighbourBlockChange(world, pos, neibpos);
            if (!(world.BlockAccessor.GetBlockEntity(pos) is BlockEntityCable blockEntityCable))
            {
                return;
            }
            Facing facing = FacingHelper.FromFace(BlockFacing.FromVector((double)(neibpos.X - pos.X), (double)(neibpos.Y - pos.Y), (double)(neibpos.Z - pos.Z)));
            if ((blockEntityCable.Connection & ~facing) == 0)
            {
                world.BlockAccessor.BreakBlock(pos, (IPlayer)null, 1f);
                return;
            }
            Facing facing2 = blockEntityCable.Switches & facing;
            if (facing2 != 0)
            {
                int num = FacingHelper.Faces(facing2).Count();
                if (num > 0)
                {
                    AssetLocation val = new AssetLocation("electricity:switch-enabled");
                    ItemStack val2 = new ItemStack(world.BlockAccessor.GetBlock(val), num);
                    world.SpawnItemEntity(val2, pos.ToVec3d(), (Vec3d)null);
                }
                blockEntityCable.Switches &= ~facing;
            }
            Facing facing3 = blockEntityCable.Connection & facing;
            if (facing3 != 0)
            {
                int num2 = FacingHelper.Count(facing3);
                if (num2 > 0)
                {
                    AssetLocation val3 = new AssetLocation("electricity:cable-part");
                    ItemStack val4 = new ItemStack(world.BlockAccessor.GetBlock(val3), num2);
                    world.SpawnItemEntity(val4, pos.ToVec3d(), (Vec3d)null);
                    blockEntityCable.Connection &= ~facing3;
                }
            }
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (this.api is ICoreClientAPI)
            {
                return true;
            }
            Vec3d hitPosition = blockSel.HitPosition;
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityCable blockEntityCable)
            {
                Facing facing = Facing.None;
                Cuboidf[] array = _dotVariant?.SelectionBoxes;
                if (array != null)
                {
                    Cuboidf[] array2 = _partVariant?.SelectionBoxes;
                    if (array2 != null)
                    {
                        Cuboidf[] array3 = _enabledSwitchVariant?.SelectionBoxes;
                        if (array3 != null)
                        {
                            Cuboidf[] array4 = _disabledSwitchVariant?.SelectionBoxes;
                            if (array4 != null)
                            {
                                foreach (KeyValuePair<Facing, Cuboidf[]> item in CalculateBoxes(key: new MeshDataKey(blockEntityCable.Connection, blockEntityCable.Switches, blockEntityCable.SwitchesState), boxesCache: SelectionBoxesCache, dotBoxes: array, partBoxes: array2, enabledSwitchBoxes: array3, disabledSwitchBoxes: array4))
                                {
                                    Facing key2 = item.Key;
                                    Cuboidf[] value = item.Value;
                                    for (int i = 0; i < value.Length; i++)
                                    {
                                        if (value[i].Clone().OmniGrowBy(0.005f).Contains(hitPosition.X, hitPosition.Y, hitPosition.Z))
                                        {
                                            facing |= key2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (BlockFacing item2 in FacingHelper.Faces(facing))
                {
                    facing |= FacingHelper.FromFace(item2);
                }
                Facing facing2 = facing & blockEntityCable.Switches;
                if (facing2 != 0)
                {
                    blockEntityCable.SwitchesState ^= facing2;
                    return true;
                }
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }

        private static void AddBoxes(ref Dictionary<Facing, Cuboidf[]> cache, Facing key, Cuboidf[] boxes)
        {
            if (cache.ContainsKey(key))
            {
                cache[key] = cache[key].Concat(boxes).ToArray();
            }
            else
            {
                cache[key] = boxes;
            }
        }

        private static Dictionary<Facing, Cuboidf[]> CalculateBoxes(IDictionary<MeshDataKey, Dictionary<Facing, Cuboidf[]>> boxesCache, Cuboidf[] dotBoxes, Cuboidf[] partBoxes, Cuboidf[] enabledSwitchBoxes, Cuboidf[] disabledSwitchBoxes, MeshDataKey key)
        {
            if (!boxesCache.TryGetValue(key, out var value))
            {
                value = (boxesCache[key] = new Dictionary<Facing, Cuboidf[]>());
                Vec3d origin = new Vec3d(0.5, 0.5, 0.5);
                if ((key.Connection & Facing.NorthAll) != 0)
                {
                    value.Add(Facing.NorthAll, dotBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 0f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.NorthEast) != 0)
                {
                    value.Add(Facing.NorthEast, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 270f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.NorthWest) != 0)
                {
                    value.Add(Facing.NorthWest, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 90f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.NorthUp) != 0)
                {
                    value.Add(Facing.NorthUp, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 0f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.NorthDown) != 0)
                {
                    value.Add(Facing.NorthDown, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 180f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.EastAll) != 0)
                {
                    value.Add(Facing.EastAll, dotBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 90f, origin)).ToArray());
                }
                if ((key.Connection & Facing.EastNorth) != 0)
                {
                    value.Add(Facing.EastNorth, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 90f, origin)).ToArray());
                }
                if ((key.Connection & Facing.EastSouth) != 0)
                {
                    value.Add(Facing.EastSouth, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(180f, 0f, 90f, origin)).ToArray());
                }
                if ((key.Connection & Facing.EastUp) != 0)
                {
                    value.Add(Facing.EastUp, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 0f, 90f, origin)).ToArray());
                }
                if ((key.Connection & Facing.EastDown) != 0)
                {
                    value.Add(Facing.EastDown, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(270f, 0f, 90f, origin)).ToArray());
                }
                if ((key.Connection & Facing.SouthAll) != 0)
                {
                    value.Add(Facing.SouthAll, dotBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(270f, 0f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.SouthEast) != 0)
                {
                    value.Add(Facing.SouthEast, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(270f, 270f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.SouthWest) != 0)
                {
                    value.Add(Facing.SouthWest, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(270f, 90f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.SouthUp) != 0)
                {
                    value.Add(Facing.SouthUp, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(270f, 180f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.SouthDown) != 0)
                {
                    value.Add(Facing.SouthDown, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(270f, 0f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.WestAll) != 0)
                {
                    value.Add(Facing.WestAll, dotBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 270f, origin)).ToArray());
                }
                if ((key.Connection & Facing.WestNorth) != 0)
                {
                    value.Add(Facing.WestNorth, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 270f, origin)).ToArray());
                }
                if ((key.Connection & Facing.WestSouth) != 0)
                {
                    value.Add(Facing.WestSouth, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(180f, 0f, 270f, origin)).ToArray());
                }
                if ((key.Connection & Facing.WestUp) != 0)
                {
                    value.Add(Facing.WestUp, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 0f, 270f, origin)).ToArray());
                }
                if ((key.Connection & Facing.WestDown) != 0)
                {
                    value.Add(Facing.WestDown, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(270f, 0f, 270f, origin)).ToArray());
                }
                if ((key.Connection & Facing.UpAll) != 0)
                {
                    value.Add(Facing.UpAll, dotBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 180f, origin)).ToArray());
                }
                if ((key.Connection & Facing.UpNorth) != 0)
                {
                    value.Add(Facing.UpNorth, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 180f, origin)).ToArray());
                }
                if ((key.Connection & Facing.UpEast) != 0)
                {
                    value.Add(Facing.UpEast, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 270f, 180f, origin)).ToArray());
                }
                if ((key.Connection & Facing.UpSouth) != 0)
                {
                    value.Add(Facing.UpSouth, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 180f, 180f, origin)).ToArray());
                }
                if ((key.Connection & Facing.UpWest) != 0)
                {
                    value.Add(Facing.UpWest, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 90f, 180f, origin)).ToArray());
                }
                if ((key.Connection & Facing.DownAll) != 0)
                {
                    value.Add(Facing.DownAll, dotBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.DownNorth) != 0)
                {
                    value.Add(Facing.DownNorth, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.DownEast) != 0)
                {
                    value.Add(Facing.DownEast, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 270f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.DownSouth) != 0)
                {
                    value.Add(Facing.DownSouth, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 180f, 0f, origin)).ToArray());
                }
                if ((key.Connection & Facing.DownWest) != 0)
                {
                    value.Add(Facing.DownWest, partBoxes.Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 90f, 0f, origin)).ToArray());
                }
                if ((key.Switches & Facing.NorthEast) != 0)
                {
                    AddBoxes(ref value, Facing.NorthAll, (((key.Switches & key.SwitchesState & Facing.NorthAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 90f, 0f, origin)).ToArray());
                }
                if ((key.Switches & Facing.NorthWest) != 0)
                {
                    AddBoxes(ref value, Facing.NorthAll, (((key.Switches & key.SwitchesState & Facing.NorthAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 270f, 0f, origin)).ToArray());
                }
                if ((key.Switches & Facing.NorthUp) != 0)
                {
                    AddBoxes(ref value, Facing.NorthAll, (((key.Switches & key.SwitchesState & Facing.NorthAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 180f, 0f, origin)).ToArray());
                }
                if ((key.Switches & Facing.NorthDown) != 0)
                {
                    AddBoxes(ref value, Facing.NorthAll, (((key.Switches & key.SwitchesState & Facing.NorthAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 0f, 0f, origin)).ToArray());
                }
                if ((key.Switches & Facing.EastNorth) != 0)
                {
                    AddBoxes(ref value, Facing.EastAll, (((key.Switches & key.SwitchesState & Facing.EastAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(180f, 0f, 90f, origin)).ToArray());
                }
                if ((key.Switches & Facing.EastSouth) != 0)
                {
                    AddBoxes(ref value, Facing.EastAll, (((key.Switches & key.SwitchesState & Facing.EastAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 90f, origin)).ToArray());
                }
                if ((key.Switches & Facing.EastUp) != 0)
                {
                    AddBoxes(ref value, Facing.EastAll, (((key.Switches & key.SwitchesState & Facing.EastAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(270f, 0f, 90f, origin)).ToArray());
                }
                if ((key.Switches & Facing.EastDown) != 0)
                {
                    AddBoxes(ref value, Facing.EastAll, (((key.Switches & key.SwitchesState & Facing.EastAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 0f, 90f, origin)).ToArray());
                }
                if ((key.Switches & Facing.SouthEast) != 0)
                {
                    AddBoxes(ref value, Facing.SouthAll, (((key.Switches & key.SwitchesState & Facing.SouthAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 90f, 180f, origin)).ToArray());
                }
                if ((key.Switches & Facing.SouthWest) != 0)
                {
                    AddBoxes(ref value, Facing.SouthAll, (((key.Switches & key.SwitchesState & Facing.SouthAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 270f, 180f, origin)).ToArray());
                }
                if ((key.Switches & Facing.SouthUp) != 0)
                {
                    AddBoxes(ref value, Facing.SouthAll, (((key.Switches & key.SwitchesState & Facing.SouthAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 180f, 180f, origin)).ToArray());
                }
                if ((key.Switches & Facing.SouthDown) != 0)
                {
                    AddBoxes(ref value, Facing.SouthAll, (((key.Switches & key.SwitchesState & Facing.SouthAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 0f, 180f, origin)).ToArray());
                }
                if ((key.Switches & Facing.WestNorth) != 0)
                {
                    AddBoxes(ref value, Facing.WestAll, (((key.Switches & key.SwitchesState & Facing.WestAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(180f, 0f, 270f, origin)).ToArray());
                }
                if ((key.Switches & Facing.WestSouth) != 0)
                {
                    AddBoxes(ref value, Facing.WestAll, (((key.Switches & key.SwitchesState & Facing.WestAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 270f, origin)).ToArray());
                }
                if ((key.Switches & Facing.WestUp) != 0)
                {
                    AddBoxes(ref value, Facing.WestAll, (((key.Switches & key.SwitchesState & Facing.WestAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(270f, 0f, 270f, origin)).ToArray());
                }
                if ((key.Switches & Facing.WestDown) != 0)
                {
                    AddBoxes(ref value, Facing.WestAll, (((key.Switches & key.SwitchesState & Facing.WestAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(90f, 0f, 270f, origin)).ToArray());
                }
                if ((key.Switches & Facing.UpNorth) != 0)
                {
                    AddBoxes(ref value, Facing.UpAll, (((key.Switches & key.SwitchesState & Facing.UpAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 180f, 180f, origin)).ToArray());
                }
                if ((key.Switches & Facing.UpEast) != 0)
                {
                    AddBoxes(ref value, Facing.UpAll, (((key.Switches & key.SwitchesState & Facing.UpAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 90f, 180f, origin)).ToArray());
                }
                if ((key.Switches & Facing.UpSouth) != 0)
                {
                    AddBoxes(ref value, Facing.UpAll, (((key.Switches & key.SwitchesState & Facing.UpAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 180f, origin)).ToArray());
                }
                if ((key.Switches & Facing.UpWest) != 0)
                {
                    AddBoxes(ref value, Facing.UpAll, (((key.Switches & key.SwitchesState & Facing.UpAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 270f, 180f, origin)).ToArray());
                }
                if ((key.Switches & Facing.DownNorth) != 0)
                {
                    AddBoxes(ref value, Facing.DownAll, (((key.Switches & key.SwitchesState & Facing.DownAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 180f, 0f, origin)).ToArray());
                }
                if ((key.Switches & Facing.DownEast) != 0)
                {
                    AddBoxes(ref value, Facing.DownAll, (((key.Switches & key.SwitchesState & Facing.DownAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 90f, 0f, origin)).ToArray());
                }
                if ((key.Switches & Facing.DownSouth) != 0)
                {
                    AddBoxes(ref value, Facing.DownAll, (((key.Switches & key.SwitchesState & Facing.DownAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 0f, 0f, origin)).ToArray());
                }
                if ((key.Switches & Facing.DownWest) != 0)
                {
                    AddBoxes(ref value, Facing.DownAll, (((key.Switches & key.SwitchesState & Facing.DownAll) != 0) ? enabledSwitchBoxes : disabledSwitchBoxes).Select((Cuboidf selectionBox) => selectionBox.RotatedCopy(0f, 270f, 0f, origin)).ToArray());
                }
            }
            return value;
        }

        private static void AddMeshData(ref MeshData sourceMesh, MeshData meshData)
        {
            if (meshData != null)
            {
                if (sourceMesh != null)
                {
                    sourceMesh.AddMeshData(meshData);
                }
                else
                {
                    sourceMesh = meshData;
                }
            }
        }

        public override void OnJsonTesselation(ref MeshData sourceMesh, ref int[] lightRgbsByCorner, BlockPos position, Block[] chunkExtBlocks, int extIndex3d)
        {
            if (this.api.World.BlockAccessor.GetBlockEntity(position) is BlockEntityCable blockEntityCable && blockEntityCable.Connection != 0)
            {
                MeshDataKey key = new MeshDataKey(blockEntityCable.Connection, blockEntityCable.Switches, blockEntityCable.SwitchesState);
                if (!MeshDataCache.TryGetValue(key, out var value))
                {
                    Vec3f val = new Vec3f(0.5f, 0.5f, 0.5f);
                    if ((key.Connection & Facing.NorthAll) != 0)
                    {
                        BlockVariant dotVariant = _dotVariant;
                        object meshData;
                        if (dotVariant == null)
                        {
                            meshData = null;
                        }
                        else
                        {
                            MeshData meshData2 = dotVariant.MeshData;
                            meshData = ((meshData2 != null) ? meshData2.Clone().Rotate(val, (float)Math.PI / 2f, 0f, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData);
                        if ((key.Connection & Facing.NorthEast) != 0)
                        {
                            BlockVariant partVariant = _partVariant;
                            object meshData3;
                            if (partVariant == null)
                            {
                                meshData3 = null;
                            }
                            else
                            {
                                MeshData meshData4 = partVariant.MeshData;
                                meshData3 = ((meshData4 != null) ? meshData4.Clone().Rotate(val, (float)Math.PI / 2f, 4.712389f, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData3);
                        }
                        if ((key.Connection & Facing.NorthWest) != 0)
                        {
                            BlockVariant partVariant2 = _partVariant;
                            object meshData5;
                            if (partVariant2 == null)
                            {
                                meshData5 = null;
                            }
                            else
                            {
                                MeshData meshData6 = partVariant2.MeshData;
                                meshData5 = ((meshData6 != null) ? meshData6.Clone().Rotate(val, (float)Math.PI / 2f, (float)Math.PI / 2f, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData5);
                        }
                        if ((key.Connection & Facing.NorthUp) != 0)
                        {
                            BlockVariant partVariant3 = _partVariant;
                            object meshData7;
                            if (partVariant3 == null)
                            {
                                meshData7 = null;
                            }
                            else
                            {
                                MeshData meshData8 = partVariant3.MeshData;
                                meshData7 = ((meshData8 != null) ? meshData8.Clone().Rotate(val, (float)Math.PI / 2f, 0f, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData7);
                        }
                        if ((key.Connection & Facing.NorthDown) != 0)
                        {
                            BlockVariant partVariant4 = _partVariant;
                            object meshData9;
                            if (partVariant4 == null)
                            {
                                meshData9 = null;
                            }
                            else
                            {
                                MeshData meshData10 = partVariant4.MeshData;
                                meshData9 = ((meshData10 != null) ? meshData10.Clone().Rotate(val, (float)Math.PI / 2f, (float)Math.PI, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData9);
                        }
                    }
                    if ((key.Connection & Facing.EastAll) != 0)
                    {
                        BlockVariant dotVariant2 = _dotVariant;
                        object meshData11;
                        if (dotVariant2 == null)
                        {
                            meshData11 = null;
                        }
                        else
                        {
                            MeshData meshData12 = dotVariant2.MeshData;
                            meshData11 = ((meshData12 != null) ? meshData12.Clone().Rotate(val, 0f, 0f, (float)Math.PI / 2f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData11);
                        if ((key.Connection & Facing.EastNorth) != 0)
                        {
                            BlockVariant partVariant5 = _partVariant;
                            object meshData13;
                            if (partVariant5 == null)
                            {
                                meshData13 = null;
                            }
                            else
                            {
                                MeshData meshData14 = partVariant5.MeshData;
                                meshData13 = ((meshData14 != null) ? meshData14.Clone().Rotate(val, 0f, 0f, (float)Math.PI / 2f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData13);
                        }
                        if ((key.Connection & Facing.EastSouth) != 0)
                        {
                            BlockVariant partVariant6 = _partVariant;
                            object meshData15;
                            if (partVariant6 == null)
                            {
                                meshData15 = null;
                            }
                            else
                            {
                                MeshData meshData16 = partVariant6.MeshData;
                                meshData15 = ((meshData16 != null) ? meshData16.Clone().Rotate(val, (float)Math.PI, 0f, (float)Math.PI / 2f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData15);
                        }
                        if ((key.Connection & Facing.EastUp) != 0)
                        {
                            BlockVariant partVariant7 = _partVariant;
                            object meshData17;
                            if (partVariant7 == null)
                            {
                                meshData17 = null;
                            }
                            else
                            {
                                MeshData meshData18 = partVariant7.MeshData;
                                meshData17 = ((meshData18 != null) ? meshData18.Clone().Rotate(val, (float)Math.PI / 2f, 0f, (float)Math.PI / 2f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData17);
                        }
                        if ((key.Connection & Facing.EastDown) != 0)
                        {
                            BlockVariant partVariant8 = _partVariant;
                            object meshData19;
                            if (partVariant8 == null)
                            {
                                meshData19 = null;
                            }
                            else
                            {
                                MeshData meshData20 = partVariant8.MeshData;
                                meshData19 = ((meshData20 != null) ? meshData20.Clone().Rotate(val, 4.712389f, 0f, (float)Math.PI / 2f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData19);
                        }
                    }
                    if ((key.Connection & Facing.SouthAll) != 0)
                    {
                        BlockVariant dotVariant3 = _dotVariant;
                        object meshData21;
                        if (dotVariant3 == null)
                        {
                            meshData21 = null;
                        }
                        else
                        {
                            MeshData meshData22 = dotVariant3.MeshData;
                            meshData21 = ((meshData22 != null) ? meshData22.Clone().Rotate(val, 4.712389f, 0f, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData21);
                        if ((key.Connection & Facing.SouthEast) != 0)
                        {
                            BlockVariant partVariant9 = _partVariant;
                            object meshData23;
                            if (partVariant9 == null)
                            {
                                meshData23 = null;
                            }
                            else
                            {
                                MeshData meshData24 = partVariant9.MeshData;
                                meshData23 = ((meshData24 != null) ? meshData24.Clone().Rotate(val, 4.712389f, 4.712389f, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData23);
                        }
                        if ((key.Connection & Facing.SouthWest) != 0)
                        {
                            BlockVariant partVariant10 = _partVariant;
                            object meshData25;
                            if (partVariant10 == null)
                            {
                                meshData25 = null;
                            }
                            else
                            {
                                MeshData meshData26 = partVariant10.MeshData;
                                meshData25 = ((meshData26 != null) ? meshData26.Clone().Rotate(val, 4.712389f, (float)Math.PI / 2f, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData25);
                        }
                        if ((key.Connection & Facing.SouthUp) != 0)
                        {
                            BlockVariant partVariant11 = _partVariant;
                            object meshData27;
                            if (partVariant11 == null)
                            {
                                meshData27 = null;
                            }
                            else
                            {
                                MeshData meshData28 = partVariant11.MeshData;
                                meshData27 = ((meshData28 != null) ? meshData28.Clone().Rotate(val, 4.712389f, (float)Math.PI, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData27);
                        }
                        if ((key.Connection & Facing.SouthDown) != 0)
                        {
                            BlockVariant partVariant12 = _partVariant;
                            object meshData29;
                            if (partVariant12 == null)
                            {
                                meshData29 = null;
                            }
                            else
                            {
                                MeshData meshData30 = partVariant12.MeshData;
                                meshData29 = ((meshData30 != null) ? meshData30.Clone().Rotate(val, 4.712389f, 0f, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData29);
                        }
                    }
                    if ((key.Connection & Facing.WestAll) != 0)
                    {
                        BlockVariant dotVariant4 = _dotVariant;
                        object meshData31;
                        if (dotVariant4 == null)
                        {
                            meshData31 = null;
                        }
                        else
                        {
                            MeshData meshData32 = dotVariant4.MeshData;
                            meshData31 = ((meshData32 != null) ? meshData32.Clone().Rotate(val, 0f, 0f, 4.712389f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData31);
                        if ((key.Connection & Facing.WestNorth) != 0)
                        {
                            BlockVariant partVariant13 = _partVariant;
                            object meshData33;
                            if (partVariant13 == null)
                            {
                                meshData33 = null;
                            }
                            else
                            {
                                MeshData meshData34 = partVariant13.MeshData;
                                meshData33 = ((meshData34 != null) ? meshData34.Clone().Rotate(val, 0f, 0f, 4.712389f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData33);
                        }
                        if ((key.Connection & Facing.WestSouth) != 0)
                        {
                            BlockVariant partVariant14 = _partVariant;
                            object meshData35;
                            if (partVariant14 == null)
                            {
                                meshData35 = null;
                            }
                            else
                            {
                                MeshData meshData36 = partVariant14.MeshData;
                                meshData35 = ((meshData36 != null) ? meshData36.Clone().Rotate(val, (float)Math.PI, 0f, 4.712389f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData35);
                        }
                        if ((key.Connection & Facing.WestUp) != 0)
                        {
                            BlockVariant partVariant15 = _partVariant;
                            object meshData37;
                            if (partVariant15 == null)
                            {
                                meshData37 = null;
                            }
                            else
                            {
                                MeshData meshData38 = partVariant15.MeshData;
                                meshData37 = ((meshData38 != null) ? meshData38.Clone().Rotate(val, (float)Math.PI / 2f, 0f, 4.712389f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData37);
                        }
                        if ((key.Connection & Facing.WestDown) != 0)
                        {
                            BlockVariant partVariant16 = _partVariant;
                            object meshData39;
                            if (partVariant16 == null)
                            {
                                meshData39 = null;
                            }
                            else
                            {
                                MeshData meshData40 = partVariant16.MeshData;
                                meshData39 = ((meshData40 != null) ? meshData40.Clone().Rotate(val, 4.712389f, 0f, 4.712389f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData39);
                        }
                    }
                    if ((key.Connection & Facing.UpAll) != 0)
                    {
                        BlockVariant dotVariant5 = _dotVariant;
                        object meshData41;
                        if (dotVariant5 == null)
                        {
                            meshData41 = null;
                        }
                        else
                        {
                            MeshData meshData42 = dotVariant5.MeshData;
                            meshData41 = ((meshData42 != null) ? meshData42.Clone().Rotate(val, 0f, 0f, (float)Math.PI) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData41);
                        if ((key.Connection & Facing.UpNorth) != 0)
                        {
                            BlockVariant partVariant17 = _partVariant;
                            object meshData43;
                            if (partVariant17 == null)
                            {
                                meshData43 = null;
                            }
                            else
                            {
                                MeshData meshData44 = partVariant17.MeshData;
                                meshData43 = ((meshData44 != null) ? meshData44.Clone().Rotate(val, 0f, 0f, (float)Math.PI) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData43);
                        }
                        if ((key.Connection & Facing.UpEast) != 0)
                        {
                            BlockVariant partVariant18 = _partVariant;
                            object meshData45;
                            if (partVariant18 == null)
                            {
                                meshData45 = null;
                            }
                            else
                            {
                                MeshData meshData46 = partVariant18.MeshData;
                                meshData45 = ((meshData46 != null) ? meshData46.Clone().Rotate(val, 0f, 4.712389f, (float)Math.PI) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData45);
                        }
                        if ((key.Connection & Facing.UpSouth) != 0)
                        {
                            BlockVariant partVariant19 = _partVariant;
                            object meshData47;
                            if (partVariant19 == null)
                            {
                                meshData47 = null;
                            }
                            else
                            {
                                MeshData meshData48 = partVariant19.MeshData;
                                meshData47 = ((meshData48 != null) ? meshData48.Clone().Rotate(val, 0f, (float)Math.PI, (float)Math.PI) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData47);
                        }
                        if ((key.Connection & Facing.UpWest) != 0)
                        {
                            BlockVariant partVariant20 = _partVariant;
                            object meshData49;
                            if (partVariant20 == null)
                            {
                                meshData49 = null;
                            }
                            else
                            {
                                MeshData meshData50 = partVariant20.MeshData;
                                meshData49 = ((meshData50 != null) ? meshData50.Clone().Rotate(val, 0f, (float)Math.PI / 2f, (float)Math.PI) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData49);
                        }
                    }
                    if ((key.Connection & Facing.DownAll) != 0)
                    {
                        BlockVariant dotVariant6 = _dotVariant;
                        object meshData51;
                        if (dotVariant6 == null)
                        {
                            meshData51 = null;
                        }
                        else
                        {
                            MeshData meshData52 = dotVariant6.MeshData;
                            meshData51 = ((meshData52 != null) ? meshData52.Clone().Rotate(val, 0f, 0f, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData51);
                        if ((key.Connection & Facing.DownNorth) != 0)
                        {
                            BlockVariant partVariant21 = _partVariant;
                            object meshData53;
                            if (partVariant21 == null)
                            {
                                meshData53 = null;
                            }
                            else
                            {
                                MeshData meshData54 = partVariant21.MeshData;
                                meshData53 = ((meshData54 != null) ? meshData54.Clone().Rotate(val, 0f, 0f, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData53);
                        }
                        if ((key.Connection & Facing.DownEast) != 0)
                        {
                            BlockVariant partVariant22 = _partVariant;
                            object meshData55;
                            if (partVariant22 == null)
                            {
                                meshData55 = null;
                            }
                            else
                            {
                                MeshData meshData56 = partVariant22.MeshData;
                                meshData55 = ((meshData56 != null) ? meshData56.Clone().Rotate(val, 0f, 4.712389f, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData55);
                        }
                        if ((key.Connection & Facing.DownSouth) != 0)
                        {
                            BlockVariant partVariant23 = _partVariant;
                            object meshData57;
                            if (partVariant23 == null)
                            {
                                meshData57 = null;
                            }
                            else
                            {
                                MeshData meshData58 = partVariant23.MeshData;
                                meshData57 = ((meshData58 != null) ? meshData58.Clone().Rotate(val, 0f, (float)Math.PI, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData57);
                        }
                        if ((key.Connection & Facing.DownWest) != 0)
                        {
                            BlockVariant partVariant24 = _partVariant;
                            object meshData59;
                            if (partVariant24 == null)
                            {
                                meshData59 = null;
                            }
                            else
                            {
                                MeshData meshData60 = partVariant24.MeshData;
                                meshData59 = ((meshData60 != null) ? meshData60.Clone().Rotate(val, 0f, (float)Math.PI / 2f, 0f) : null);
                            }
                            AddMeshData(ref value, (MeshData)meshData59);
                        }
                    }
                    if ((key.Switches & Facing.NorthEast) != 0)
                    {
                        BlockVariant obj = (((key.Switches & key.SwitchesState & Facing.NorthAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData61;
                        if (obj == null)
                        {
                            meshData61 = null;
                        }
                        else
                        {
                            MeshData meshData62 = obj.MeshData;
                            meshData61 = ((meshData62 != null) ? meshData62.Clone().Rotate(val, (float)Math.PI / 2f, (float)Math.PI / 2f, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData61);
                    }
                    if ((key.Switches & Facing.NorthWest) != 0)
                    {
                        BlockVariant obj2 = (((key.Switches & key.SwitchesState & Facing.NorthAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData63;
                        if (obj2 == null)
                        {
                            meshData63 = null;
                        }
                        else
                        {
                            MeshData meshData64 = obj2.MeshData;
                            meshData63 = ((meshData64 != null) ? meshData64.Clone().Rotate(val, (float)Math.PI / 2f, 4.712389f, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData63);
                    }
                    if ((key.Switches & Facing.NorthUp) != 0)
                    {
                        BlockVariant obj3 = (((key.Switches & key.SwitchesState & Facing.NorthAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData65;
                        if (obj3 == null)
                        {
                            meshData65 = null;
                        }
                        else
                        {
                            MeshData meshData66 = obj3.MeshData;
                            meshData65 = ((meshData66 != null) ? meshData66.Clone().Rotate(val, (float)Math.PI / 2f, (float)Math.PI, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData65);
                    }
                    if ((key.Switches & Facing.NorthDown) != 0)
                    {
                        BlockVariant obj4 = (((key.Switches & key.SwitchesState & Facing.NorthAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData67;
                        if (obj4 == null)
                        {
                            meshData67 = null;
                        }
                        else
                        {
                            MeshData meshData68 = obj4.MeshData;
                            meshData67 = ((meshData68 != null) ? meshData68.Clone().Rotate(val, (float)Math.PI / 2f, 0f, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData67);
                    }
                    if ((key.Switches & Facing.EastNorth) != 0)
                    {
                        BlockVariant obj5 = (((key.Switches & key.SwitchesState & Facing.EastAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData69;
                        if (obj5 == null)
                        {
                            meshData69 = null;
                        }
                        else
                        {
                            MeshData meshData70 = obj5.MeshData;
                            meshData69 = ((meshData70 != null) ? meshData70.Clone().Rotate(val, (float)Math.PI, 0f, (float)Math.PI / 2f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData69);
                    }
                    if ((key.Switches & Facing.EastSouth) != 0)
                    {
                        BlockVariant obj6 = (((key.Switches & key.SwitchesState & Facing.EastAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData71;
                        if (obj6 == null)
                        {
                            meshData71 = null;
                        }
                        else
                        {
                            MeshData meshData72 = obj6.MeshData;
                            meshData71 = ((meshData72 != null) ? meshData72.Clone().Rotate(val, 0f, 0f, (float)Math.PI / 2f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData71);
                    }
                    if ((key.Switches & Facing.EastUp) != 0)
                    {
                        BlockVariant obj7 = (((key.Switches & key.SwitchesState & Facing.EastAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData73;
                        if (obj7 == null)
                        {
                            meshData73 = null;
                        }
                        else
                        {
                            MeshData meshData74 = obj7.MeshData;
                            meshData73 = ((meshData74 != null) ? meshData74.Clone().Rotate(val, 4.712389f, 0f, (float)Math.PI / 2f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData73);
                    }
                    if ((key.Switches & Facing.EastDown) != 0)
                    {
                        BlockVariant obj8 = (((key.Switches & key.SwitchesState & Facing.EastAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData75;
                        if (obj8 == null)
                        {
                            meshData75 = null;
                        }
                        else
                        {
                            MeshData meshData76 = obj8.MeshData;
                            meshData75 = ((meshData76 != null) ? meshData76.Clone().Rotate(val, (float)Math.PI / 2f, 0f, (float)Math.PI / 2f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData75);
                    }
                    if ((key.Switches & Facing.SouthEast) != 0)
                    {
                        BlockVariant obj9 = (((key.Switches & key.SwitchesState & Facing.SouthAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData77;
                        if (obj9 == null)
                        {
                            meshData77 = null;
                        }
                        else
                        {
                            MeshData meshData78 = obj9.MeshData;
                            meshData77 = ((meshData78 != null) ? meshData78.Clone().Rotate(val, (float)Math.PI / 2f, (float)Math.PI / 2f, (float)Math.PI) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData77);
                    }
                    if ((key.Switches & Facing.SouthWest) != 0)
                    {
                        BlockVariant obj10 = (((key.Switches & key.SwitchesState & Facing.SouthAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData79;
                        if (obj10 == null)
                        {
                            meshData79 = null;
                        }
                        else
                        {
                            MeshData meshData80 = obj10.MeshData;
                            meshData79 = ((meshData80 != null) ? meshData80.Clone().Rotate(val, (float)Math.PI / 2f, 4.712389f, (float)Math.PI) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData79);
                    }
                    if ((key.Switches & Facing.SouthUp) != 0)
                    {
                        BlockVariant obj11 = (((key.Switches & key.SwitchesState & Facing.SouthAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData81;
                        if (obj11 == null)
                        {
                            meshData81 = null;
                        }
                        else
                        {
                            MeshData meshData82 = obj11.MeshData;
                            meshData81 = ((meshData82 != null) ? meshData82.Clone().Rotate(val, (float)Math.PI / 2f, (float)Math.PI, (float)Math.PI) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData81);
                    }
                    if ((key.Switches & Facing.SouthDown) != 0)
                    {
                        BlockVariant obj12 = (((key.Switches & key.SwitchesState & Facing.SouthAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData83;
                        if (obj12 == null)
                        {
                            meshData83 = null;
                        }
                        else
                        {
                            MeshData meshData84 = obj12.MeshData;
                            meshData83 = ((meshData84 != null) ? meshData84.Clone().Rotate(val, (float)Math.PI / 2f, 0f, (float)Math.PI) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData83);
                    }
                    if ((key.Switches & Facing.WestNorth) != 0)
                    {
                        BlockVariant obj13 = (((key.Switches & key.SwitchesState & Facing.WestAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData85;
                        if (obj13 == null)
                        {
                            meshData85 = null;
                        }
                        else
                        {
                            MeshData meshData86 = obj13.MeshData;
                            meshData85 = ((meshData86 != null) ? meshData86.Clone().Rotate(val, (float)Math.PI, 0f, 4.712389f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData85);
                    }
                    if ((key.Switches & Facing.WestSouth) != 0)
                    {
                        BlockVariant obj14 = (((key.Switches & key.SwitchesState & Facing.WestAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData87;
                        if (obj14 == null)
                        {
                            meshData87 = null;
                        }
                        else
                        {
                            MeshData meshData88 = obj14.MeshData;
                            meshData87 = ((meshData88 != null) ? meshData88.Clone().Rotate(val, 0f, 0f, 4.712389f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData87);
                    }
                    if ((key.Switches & Facing.WestUp) != 0)
                    {
                        BlockVariant obj15 = (((key.Switches & key.SwitchesState & Facing.WestAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData89;
                        if (obj15 == null)
                        {
                            meshData89 = null;
                        }
                        else
                        {
                            MeshData meshData90 = obj15.MeshData;
                            meshData89 = ((meshData90 != null) ? meshData90.Clone().Rotate(val, 4.712389f, 0f, 4.712389f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData89);
                    }
                    if ((key.Switches & Facing.WestDown) != 0)
                    {
                        BlockVariant obj16 = (((key.Switches & key.SwitchesState & Facing.WestAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData91;
                        if (obj16 == null)
                        {
                            meshData91 = null;
                        }
                        else
                        {
                            MeshData meshData92 = obj16.MeshData;
                            meshData91 = ((meshData92 != null) ? meshData92.Clone().Rotate(val, (float)Math.PI / 2f, 0f, 4.712389f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData91);
                    }
                    if ((key.Switches & Facing.UpNorth) != 0)
                    {
                        BlockVariant obj17 = (((key.Switches & key.SwitchesState & Facing.UpAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData93;
                        if (obj17 == null)
                        {
                            meshData93 = null;
                        }
                        else
                        {
                            MeshData meshData94 = obj17.MeshData;
                            meshData93 = ((meshData94 != null) ? meshData94.Clone().Rotate(val, 0f, (float)Math.PI, (float)Math.PI) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData93);
                    }
                    if ((key.Switches & Facing.UpEast) != 0)
                    {
                        BlockVariant obj18 = (((key.Switches & key.SwitchesState & Facing.UpAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData95;
                        if (obj18 == null)
                        {
                            meshData95 = null;
                        }
                        else
                        {
                            MeshData meshData96 = obj18.MeshData;
                            meshData95 = ((meshData96 != null) ? meshData96.Clone().Rotate(val, 0f, (float)Math.PI / 2f, (float)Math.PI) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData95);
                    }
                    if ((key.Switches & Facing.UpSouth) != 0)
                    {
                        BlockVariant obj19 = (((key.Switches & key.SwitchesState & Facing.UpAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData97;
                        if (obj19 == null)
                        {
                            meshData97 = null;
                        }
                        else
                        {
                            MeshData meshData98 = obj19.MeshData;
                            meshData97 = ((meshData98 != null) ? meshData98.Clone().Rotate(val, 0f, 0f, (float)Math.PI) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData97);
                    }
                    if ((key.Switches & Facing.UpWest) != 0)
                    {
                        BlockVariant obj20 = (((key.Switches & key.SwitchesState & Facing.UpAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData99;
                        if (obj20 == null)
                        {
                            meshData99 = null;
                        }
                        else
                        {
                            MeshData meshData100 = obj20.MeshData;
                            meshData99 = ((meshData100 != null) ? meshData100.Clone().Rotate(val, 0f, 4.712389f, (float)Math.PI) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData99);
                    }
                    if ((key.Switches & Facing.DownNorth) != 0)
                    {
                        BlockVariant obj21 = (((key.Switches & key.SwitchesState & Facing.DownAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData101;
                        if (obj21 == null)
                        {
                            meshData101 = null;
                        }
                        else
                        {
                            MeshData meshData102 = obj21.MeshData;
                            meshData101 = ((meshData102 != null) ? meshData102.Clone().Rotate(val, 0f, (float)Math.PI, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData101);
                    }
                    if ((key.Switches & Facing.DownEast) != 0)
                    {
                        BlockVariant obj22 = (((key.Switches & key.SwitchesState & Facing.DownAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData103;
                        if (obj22 == null)
                        {
                            meshData103 = null;
                        }
                        else
                        {
                            MeshData meshData104 = obj22.MeshData;
                            meshData103 = ((meshData104 != null) ? meshData104.Clone().Rotate(val, 0f, (float)Math.PI / 2f, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData103);
                    }
                    if ((key.Switches & Facing.DownSouth) != 0)
                    {
                        BlockVariant obj23 = (((key.Switches & key.SwitchesState & Facing.DownAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData105;
                        if (obj23 == null)
                        {
                            meshData105 = null;
                        }
                        else
                        {
                            MeshData meshData106 = obj23.MeshData;
                            meshData105 = ((meshData106 != null) ? meshData106.Clone().Rotate(val, 0f, 0f, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData105);
                    }
                    if ((key.Switches & Facing.DownWest) != 0)
                    {
                        BlockVariant obj24 = (((key.Switches & key.SwitchesState & Facing.DownAll) != 0) ? _enabledSwitchVariant : _disabledSwitchVariant);
                        object meshData107;
                        if (obj24 == null)
                        {
                            meshData107 = null;
                        }
                        else
                        {
                            MeshData meshData108 = obj24.MeshData;
                            meshData107 = ((meshData108 != null) ? meshData108.Clone().Rotate(val, 0f, 4.712389f, 0f) : null);
                        }
                        AddMeshData(ref value, (MeshData)meshData107);
                    }
                    MeshDataCache[key] = value;
                }
                sourceMesh = value ?? sourceMesh;
            }
            base.OnJsonTesselation(ref sourceMesh, ref lightRgbsByCorner, position, chunkExtBlocks, extIndex3d);
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos position)
        {
            Cuboidf[] array = _dotVariant?.SelectionBoxes;
            if (array != null)
            {
                Cuboidf[] array2 = _partVariant?.SelectionBoxes;
                if (array2 != null)
                {
                    Cuboidf[] array3 = _enabledSwitchVariant?.SelectionBoxes;
                    if (array3 != null)
                    {
                        Cuboidf[] array4 = _disabledSwitchVariant?.SelectionBoxes;
                        if (array4 != null && blockAccessor.GetBlockEntity(position) is BlockEntityCable blockEntityCable)
                        {
                            return CalculateBoxes(key: new MeshDataKey(blockEntityCable.Connection, blockEntityCable.Switches, blockEntityCable.SwitchesState), boxesCache: SelectionBoxesCache, dotBoxes: array, partBoxes: array2, enabledSwitchBoxes: array3, disabledSwitchBoxes: array4).Values.SelectMany((Cuboidf[] x) => x).Distinct().ToArray();
                        }
                    }
                }
            }
            return base.GetSelectionBoxes(blockAccessor, position);
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos position)
        {
            Cuboidf[] array = _dotVariant?.CollisionBoxes;
            if (array != null)
            {
                Cuboidf[] array2 = _partVariant?.CollisionBoxes;
                if (array2 != null)
                {
                    Cuboidf[] array3 = _enabledSwitchVariant?.CollisionBoxes;
                    if (array3 != null)
                    {
                        Cuboidf[] array4 = _disabledSwitchVariant?.CollisionBoxes;
                        if (array4 != null && blockAccessor.GetBlockEntity(position) is BlockEntityCable blockEntityCable)
                        {
                            return CalculateBoxes(key: new MeshDataKey(blockEntityCable.Connection, blockEntityCable.Switches, blockEntityCable.SwitchesState), boxesCache: CollisionBoxesCache, dotBoxes: array, partBoxes: array2, enabledSwitchBoxes: array3, disabledSwitchBoxes: array4).Values.SelectMany((Cuboidf[] x) => x).Distinct().ToArray();
                        }
                    }
                }
            }
            return base.GetSelectionBoxes(blockAccessor, position);
        }
    }
}
