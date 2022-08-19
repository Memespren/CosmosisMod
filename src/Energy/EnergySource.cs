using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class EnergySource : EnergyBlockEntity
    {
        private Dictionary<EnergySink, EnergyPath> paths;

        private Queue<EnergySink> toServe;

        private EnergyPath alignedPath;

        public EnergySource() : base()
        {
            paths = new Dictionary<EnergySink, EnergyPath>();
            toServe = new Queue<EnergySink>();
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RegisterGameTickListener(OnGameTick, 250);
        }

        public void OnGameTick(float dt)
        {
            if (alignedPath != null)
            {
                alignedPath.Trigger();
                alignedPath = null;
            }
            if (toServe.Count > 0)
            {
                EnergySink sink = toServe.Dequeue();
                if(paths.ContainsKey(sink)){
                    paths[sink].Align();
                    alignedPath = paths[sink];
                    toServe.Enqueue(sink);
                }
            }
        }

        public void AddPath(EnergySink sink, LinkedList<EnergyNode> nodes)
        {
            if(!paths.ContainsKey(sink) || nodes.Count < paths[sink].Length)
            {
                paths[sink] = new EnergyPath(this, sink, nodes);
                if (!toServe.Contains(sink))
                    toServe.Enqueue(sink);
            }
        }

        public void RemovePath(EnergyPath path)
        {
            if(paths.ContainsValue(path))
                paths.Remove(path.sink);
        }
    }
}