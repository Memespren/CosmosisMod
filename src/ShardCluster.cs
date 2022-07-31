using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class ShardCluster : Block
    {
        public override bool TryPlaceBlockForWorldGen(IBlockAccessor blockAccessor, BlockPos pos, BlockFacing onBlockFace, LCGRandom worldgenRandom)
        {
            if (blockAccessor.GetSolidBlock(pos.X, pos.Y-1, pos.Z).SideSolid[BlockFacing.indexUP])
            {
                blockAccessor.SetBlock(BlockId, pos);
                return true;
            }
            return false;
        }
    }
}