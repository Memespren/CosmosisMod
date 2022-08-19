using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class EnergyBlockEntity : BlockEntity
    {
        public event Action<EnergyBlockEntity> OnRemoved;

        public HashSet<EnergyBlockEntity> neighbors;

        protected int range = 5;

        public EnergyBlockEntity()
        {
            neighbors = new HashSet<EnergyBlockEntity>();
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (neighbors == null)
                neighbors = new HashSet<EnergyBlockEntity>();
            for(int i = -range; i <= range; i++)
            {
                for(int j = -range; j <= range; j++)
                {
                    for(int k = -range; k <= range; k++)
                    {
                        BlockPos check = Pos.AddCopy(i, j, k);
                        EnergyBlockEntity ebe = Api.World.BlockAccessor.GetBlockEntity(check) as EnergyBlockEntity;
                        if (ebe != null && ebe != this)
                        {
                            AddNeighbor(ebe);
                            ebe.AddNeighbor(this);
                        }
                    }
                }
            }
            RecalculatePaths(new HashSet<EnergyBlockEntity>());
        }
        public void RecalculatePaths(HashSet<EnergyBlockEntity> visited)
        {
            visited.Add(this);
            foreach (EnergyBlockEntity ebe in neighbors)
            {
                (ebe as EnergySink)?.FindPath();
                if(!visited.Contains(ebe))
                    (ebe as EnergyNode)?.RecalculatePaths(visited);

            }
        }

        public void AddNeighbor(EnergyBlockEntity neighbor)
        {
            neighbors.Add(neighbor);
            //Api.Logger.Debug("Neighbors: " + neighbors.Count);
        }

        public void RemoveNeighbor(EnergyBlockEntity neighbor)
        {
            neighbors.Remove(neighbor);
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            foreach (EnergyBlockEntity ebe in neighbors)
            {
                ebe.RemoveNeighbor(this);
            }
            OnRemoved?.Invoke(this);
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            foreach (EnergyBlockEntity ebe in neighbors)
            {
                ebe.RemoveNeighbor(this);
            }
            OnRemoved?.Invoke(this);
        }
    }
}