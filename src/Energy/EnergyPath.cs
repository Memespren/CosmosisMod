using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class EnergyPath
    {
        private List<EnergyNode> nodes;

        public EnergySource source;

        public EnergySink sink;

        public EnergyPath(EnergySource source, EnergySink sink, LinkedList<EnergyNode> nodeStack)
        {
            this.source = source;
            source.OnRemoved += SourceRemoved;

            this.sink = sink;
            sink.OnRemoved += SinkRemoved;

            this.nodes = new List<EnergyNode>();
            foreach (EnergyNode node in nodeStack)
            {
                this.nodes.Add(node);
                node.OnRemoved += NodeRemoved;
            }
        }

        public void NodeRemoved(EnergyBlockEntity node)
        {
            Delete();
            sink.FindPath();
        }

        public void SinkRemoved(EnergyBlockEntity sink)
        {
            Delete();
        }

        public void SourceRemoved(EnergyBlockEntity source)
        {
            Delete();
        }

        public void Delete()
        {
            source.RemovePath(this);
            source.OnRemoved -= SourceRemoved;
            sink.OnRemoved -= SinkRemoved;
            foreach (EnergyNode node in nodes)
            {
                node.OnRemoved -= NodeRemoved;
            }
        }

        public void Align()
        {
            for(int i = 0; i < nodes.Count-1; ++i)
            {
                nodes[i].AlignTo(nodes[i+1].Pos);
            }
            nodes[nodes.Count-1].AlignTo(sink.Pos);
        }

        public void Trigger()
        {
            foreach(EnergyNode node in nodes)
            {
                node.Trigger();
            }
        }

        public int Length
        {
            get { return nodes.Count; }
        }
    }
}