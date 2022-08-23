using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class EnergySink : EnergyBlockEntity
    {
        protected int energy = 0;
        protected int capacity = 0;
        protected int energyPerOperation = 0;

        public virtual void Setup(int capacity, int energyPerOperation, int msPerOperation)
        {
            this.capacity = capacity;
            this.energyPerOperation = energyPerOperation;
            RegisterGameTickListener(TryOperation, msPerOperation);
        }

        public void TryOperation(float dt)
        {
            if(energy >= energyPerOperation && DoOperation())
                energy -= energyPerOperation;
        }   

        public virtual bool DoOperation()
        {
            return false;
        }

        public int SendEnergy(int energyIn)
        {
            int amount = Math.Min(capacity-energy, energyIn);
            energy += amount;
            return amount;
        }

        public bool isFull()
        {
            return (energy >= capacity && capacity > 0);
        }

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