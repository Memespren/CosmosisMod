using System;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace cosmosis
{
    public class LGItemSlot : ItemSlot
    {
        public LGItemSlot(LookingGlassInventory inventory) : base(inventory)
        {
        }

        public override void ActivateSlot(ItemSlot sourceSlot, ref ItemStackMoveOperation op)
        {
            (Inventory as LookingGlassInventory).SlotClicked(this, ref op);
        }
        public override bool CanTake()
        {
            return false;
        }
    }
}