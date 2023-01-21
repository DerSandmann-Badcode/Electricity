using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.MathTools;

namespace Electricity.Utils
{
    public static class FacingHelper
    {
        public static Facing FromFace(BlockFacing face)
        {
            switch (face.Index)
            {
                case 0:
                    return Facing.NorthAll;
                case 1:
                    return Facing.EastAll;
                case 2:
                    return Facing.SouthAll;
                case 3:
                    return Facing.WestAll;
                case 4:
                    return Facing.UpAll;
                case 5:
                    return Facing.DownAll;
                default:
                    return Facing.None;
            }
        }

        public static Facing FromDirection(BlockFacing direction)
        {
            switch (direction.Index)
            {
                case 0:
                    return Facing.AllNorth;
                case 1:
                    return Facing.AllEast;
                case 2:
                    return Facing.AllSouth;
                case 3:
                    return Facing.AllWest;
                case 4:
                    return Facing.AllUp;
                case 5:
                    return Facing.AllDown;
                default:
                    return Facing.None;
            }
        }

        public static Facing From(BlockFacing face, BlockFacing direction)
        {
            return FromFace(face) & FromDirection(direction);
        }

        public static Facing OppositeDirection(Facing self)
        {
            return Facing.None | (((self & Facing.NorthEast) != 0) ? Facing.NorthWest : Facing.None) | (((self & Facing.NorthWest) != 0) ? Facing.NorthEast : Facing.None) | (((self & Facing.NorthUp) != 0) ? Facing.NorthDown : Facing.None) | (((self & Facing.NorthDown) != 0) ? Facing.NorthUp : Facing.None) | (((self & Facing.EastNorth) != 0) ? Facing.EastSouth : Facing.None) | (((self & Facing.EastSouth) != 0) ? Facing.EastNorth : Facing.None) | (((self & Facing.EastUp) != 0) ? Facing.EastDown : Facing.None) | (((self & Facing.EastDown) != 0) ? Facing.EastUp : Facing.None) | (((self & Facing.SouthEast) != 0) ? Facing.SouthWest : Facing.None) | (((self & Facing.SouthWest) != 0) ? Facing.SouthEast : Facing.None) | (((self & Facing.SouthUp) != 0) ? Facing.SouthDown : Facing.None) | (((self & Facing.SouthDown) != 0) ? Facing.SouthUp : Facing.None) | (((self & Facing.UpNorth) != 0) ? Facing.UpSouth : Facing.None) | (((self & Facing.UpEast) != 0) ? Facing.UpWest : Facing.None) | (((self & Facing.UpSouth) != 0) ? Facing.UpNorth : Facing.None) | (((self & Facing.UpWest) != 0) ? Facing.UpEast : Facing.None) | (((self & Facing.DownNorth) != 0) ? Facing.DownSouth : Facing.None) | (((self & Facing.DownEast) != 0) ? Facing.DownWest : Facing.None) | (((self & Facing.DownSouth) != 0) ? Facing.DownNorth : Facing.None) | (((self & Facing.DownWest) != 0) ? Facing.DownEast : Facing.None);
        }

        public static IEnumerable<BlockFacing> Faces(Facing self)
        {
            return from face in (IEnumerable<BlockFacing>)(object)new BlockFacing[6]
            {
                ((self & Facing.NorthAll) != 0) ? BlockFacing.NORTH : null,
                ((self & Facing.EastAll) != 0) ? BlockFacing.EAST : null,
                ((self & Facing.SouthAll) != 0) ? BlockFacing.SOUTH : null,
                ((self & Facing.WestAll) != 0) ? BlockFacing.WEST : null,
                ((self & Facing.UpAll) != 0) ? BlockFacing.UP : null,
                ((self & Facing.DownAll) != 0) ? BlockFacing.DOWN : null
            } where face != null select (face);
        }

        public static IEnumerable<BlockFacing> Directions(Facing self)
        {
            return from face in (IEnumerable<BlockFacing>)(object)new BlockFacing[6]
            {
                ((self & Facing.AllNorth) != 0) ? BlockFacing.NORTH : null,
                ((self & Facing.AllEast) != 0) ? BlockFacing.EAST : null,
                ((self & Facing.AllSouth) != 0) ? BlockFacing.SOUTH : null,
                ((self & Facing.AllWest) != 0) ? BlockFacing.WEST : null,
                ((self & Facing.AllUp) != 0) ? BlockFacing.UP : null,
                ((self & Facing.AllDown) != 0) ? BlockFacing.DOWN : null
            } where face != null select (face);
        }

        public static Facing FullFace(Facing self)
        {
            return Faces(self).Aggregate(Facing.None, (Facing current, BlockFacing face) => current | FromFace(face));
        }

        public static int Count(Facing self)
        {
            int num = 0;
            while (self != 0)
            {
                num++;
                self &= self - 1;
            }
            return num;
        }
    }
}
