using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class EnergySink : EnergyBlockEntity
    {
        public void FindPath()
        {
            foreach (EnergyBlockEntity ebe in neighbors)
            {
                (ebe as EnergyNode)?.BuildPath(this, new LinkedList<EnergyNode>());
            }
        }

        public override bool CheckNeighbor(EnergyBlockEntity other)
        {
            return (other is EnergyNode);
        }
    }
}