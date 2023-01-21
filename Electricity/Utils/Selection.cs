using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace Electricity.Utils
{
    public class Selection
    {
        private readonly Vec3d _hitPosition;
        private readonly bool _didOffset;

        public Vec2d Position2d
        {
            get
            {
                switch(Face.Index)
                {
                    case 0:
                    case 2:
                        return new Vec2d(_hitPosition.X, _hitPosition.Y);
                    case 1:
                    case 3:
                        return new Vec2d(_hitPosition.Y, _hitPosition.Z);
                    case 4:
                    case 5:
                        return new Vec2d(_hitPosition.X, _hitPosition.Z);
                    default:
                        throw new Exception();
                }
            }
        }

        public BlockFacing Direction
        {
            get
            {
                switch (Face.Index)
                {
                    case 0:
                    case 2:
                        return DirectionHelper(BlockFacing.EAST, BlockFacing.WEST, BlockFacing.UP, BlockFacing.DOWN);
                    case 1:
                    case 3:
                        return DirectionHelper(BlockFacing.UP, BlockFacing.DOWN, BlockFacing.SOUTH, BlockFacing.NORTH);
                    case 4:
                    case 5:
                        return DirectionHelper(BlockFacing.EAST, BlockFacing.WEST, BlockFacing.SOUTH, BlockFacing.NORTH);
                    default:
                        throw new Exception();
                }
            }
        }

        public BlockFacing Face
        {
            get
            {
                Vec3d val = _hitPosition.SubCopy(0.5, 0.5, 0.5);
                if (val.X > val.Y && val.X > val.Z && val.X > 0.0 - val.Y && val.X > 0.0 - val.Z)
                {
                    if (!_didOffset)
                    {
                        return BlockFacing.EAST;
                    }
                    return BlockFacing.WEST;
                }
                if (val.X < val.Y && val.X < val.Z && val.X < 0.0 - val.Y && val.X < 0.0 - val.Z)
                {
                    if (!_didOffset)
                    {
                        return BlockFacing.WEST;
                    }
                    return BlockFacing.EAST;
                }
                if (val.Z > val.Y && val.Z > val.X && val.Z > 0.0 - val.Y && val.Z > 0.0 - val.X)
                {
                    if (!_didOffset)
                    {
                        return BlockFacing.SOUTH;
                    }
                    return BlockFacing.NORTH;
                }
                if (val.Z < val.Y && val.Z < val.X && val.Z < 0.0 - val.Y && val.Z < 0.0 - val.X)
                {
                    if (!_didOffset)
                    {
                        return BlockFacing.NORTH;
                    }
                    return BlockFacing.SOUTH;
                }
                if (val.Y > val.X && val.Y > val.Z && val.Y > 0.0 - val.X && val.Y > 0.0 - val.Z)
                {
                    if (!_didOffset)
                    {
                        return BlockFacing.UP;
                    }
                    return BlockFacing.DOWN;
                }
                if (val.Y < val.X && val.Y < val.Z && val.Y < 0.0 - val.X && val.Y < 0.0 - val.Z)
                {
                    if (!_didOffset)
                    {
                        return BlockFacing.DOWN;
                    }
                    return BlockFacing.UP;
                }
                throw new Exception();
            }
        }

        public Facing Facing => FacingHelper.From(Face, Direction);

        public Selection(Vec3d hitPosition, bool didOffset)
        {
            _hitPosition = hitPosition;
            _didOffset = didOffset;
        }

        public Selection(BlockSelection blockSelection)
        {
            _hitPosition = blockSelection.HitPosition;
            _didOffset = blockSelection.DidOffset;
        }

        private static Vec2d Rotate(Vec2d point, Vec2d origin, double angle)
        {
            //IL_0060: Unknown result type (might be due to invalid IL or missing references)
            //IL_0066: Expected O, but got Unknown
            return new Vec2d(GameMath.Cos(angle) * (point.X - origin.X) - GameMath.Sin(angle) * (point.Y - origin.Y) + origin.X, GameMath.Sin(angle) * (point.X - origin.X) + GameMath.Cos(angle) * (point.Y - origin.Y) + origin.Y);
        }

        private BlockFacing DirectionHelper(params BlockFacing[] mapping)
        {
            //IL_0018: Unknown result type (might be due to invalid IL or missing references)
            //IL_002b: Expected O, but got Unknown
            Vec2d val = Rotate(Position2d, new Vec2d(0.5, 0.5), 0.7853981573134661);
            if (val.X > 0.5 && val.Y > 0.5)
            {
                return mapping[0];
            }
            if (val.X < 0.5 && val.Y < 0.5)
            {
                return mapping[1];
            }
            if (val.X < 0.5 && val.Y > 0.5)
            {
                return mapping[2];
            }
            if (val.X > 0.5 && val.Y < 0.5)
            {
                return mapping[3];
            }
            throw new Exception();
        }
    }
}
