using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;


namespace cosmosis
{
    public class LookingGlassInventory : InventoryBase, ISlotProvider
    {
        ItemSlot[] slots;
        public ItemSlot[] Slots {get {return slots;}}

        public BELookingGlass belg;

        public int usedSize;

        public LookingGlassInventory(string inventoryID, ICoreAPI api, int size, BELookingGlass belg) : base(inventoryID, api)
        {
            slots = GenEmptySlots(size*2);
            usedSize = size;
            this.belg = belg;
        }

        public void Reset(int size)
        {
            DiscardAll();
            if (size > slots.Length)
            {
                int arrayLength = slots.Length;
                Array.Resize<ItemSlot>(ref slots, size*2);
                ItemSlot[] newSlots = GenEmptySlots(size*2 - arrayLength);
                for(int i = arrayLength; i < size*2; ++i)
                {
                    slots[i] = newSlots[i-arrayLength];
                }
            }
            usedSize = size;
            
        }

        public void SlotClicked(ItemSlot slot, ref ItemStackMoveOperation op)
        {
            belg.RequestItem(slot, ref op);
        }

        protected override ItemSlot NewSlot(int i)
        {
            return new LGItemSlot(this);
        }

        public override int Count
        {
            get {return usedSize;}
        }

        public override ItemSlot this[int slotId]
        {
            get
            {
                if (slotId < 0 || slotId >= slots.Length) return null;
                return slots[slotId];
            }
            set
            {
                if (slotId < 0 || slotId >= slots.Length) throw new ArgumentOutOfRangeException(nameof(slotId));
                if (value == null) throw new ArgumentNullException(nameof(value));
                slots[slotId] = value;
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree)
        {
            slots = SlotsFromTreeAttributes(tree, slots);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            SlotsToTreeAttributes(slots, tree);
        }
    }
}