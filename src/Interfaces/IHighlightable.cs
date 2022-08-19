using System.Collections.Generic;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public interface IHighlightable
    {
        void GetHighlightedBlocks(ref List<BlockPos> blueList, ref List<BlockPos> orangeList);
    }
}