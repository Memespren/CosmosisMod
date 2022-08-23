using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class EnergyNode : EnergyBlockEntity
    {
        public override void Initialize(ICoreAPI api)
        {
            neighbors = new HashSet<EnergyBlockEntity>();
            base.Initialize(api);
        }
        
        public void BuildPath(EnergySink sink, LinkedList<EnergyNode> nodes)
        {
            nodes.AddFirst(this);
            foreach (EnergyBlockEntity ebe in neighbors)
            {
                EnergyNode node = ebe as EnergyNode;
                if (node != null && !nodes.Contains(node))
                    node.BuildPath(sink, new LinkedList<EnergyNode>(nodes));

                if (ebe is EnergySource)
                    (ebe as EnergySource).AddPath(sink, nodes);
            }
        }

        public virtual bool AlignTo(EnergyBlockEntity target)
        {
            return true;
        }

        public virtual void Trigger()
        {
            
        }
    }
}