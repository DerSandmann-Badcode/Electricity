using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Electricity.Utils
{
    internal class BlockVariant
    {
        public readonly Cuboidf[] CollisionBoxes;

        public readonly Cuboidf[] SelectionBoxes;

        public readonly MeshData MeshData;

        public BlockVariant(ICoreAPI coreApi, CollectibleObject baseBlock, string variant)
        {
            Block block = coreApi.World.GetBlock(((RegistryObject)baseBlock).CodeWithVariant("type", variant));
            CollisionBoxes = block.CollisionBoxes;
            SelectionBoxes = block.SelectionBoxes;
            ICoreClientAPI val = (ICoreClientAPI)(object)((coreApi is ICoreClientAPI) ? coreApi : null);
            if (val != null)
            {
                Shape cachedShape = val.TesselatorManager.GetCachedShape(block.Shape.Base);
                val.Tesselator.TesselateShape(baseBlock, cachedShape, out MeshData, (Vec3f)null, (int?)null, (string[])null);
            }
        }
    }
}
