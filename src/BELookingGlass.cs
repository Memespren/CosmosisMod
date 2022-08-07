using System.IO;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Datastructures;


namespace cosmosis
{
    public class BELookingGlass : NetworkBlockEntity
    {

        public LookingGlassInventory inventory;
        public LookingGlassGui gui;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            connectedNetwork.ContentsModified += ReloadGui;
        }

        public void OpenGUI()
        {
            BuildInventory();
            if (Api is ICoreClientAPI)
            {
                gui = new LookingGlassGui("Looking Glass", inventory, Pos, Api as ICoreClientAPI);
                gui.TryOpen();
            }
        }

        public void RequestItem(ItemSlot slot, ref ItemStackMoveOperation op)
        {
            if (slot != null && slot.Itemstack != null && op.ActingPlayer != null)
            {
                //Api.Logger.Debug(slot.Itemstack.GetName() + " Requested By " + op.ActingPlayer.PlayerName);
                CosmosisMod.requestClientChannel.SendPacket<ItemRequestPacket>(new ItemRequestPacket(){
                    X = Pos.X,
                    Y = Pos.Y,
                    Z = Pos.Z,
                    slotID = inventory.GetSlotId(slot),
                    itemCode = slot.Itemstack.Collectible.Code.ToString(),
                    playerUID = op.ActingPlayer.PlayerUID,
                    ctrl = op.CtrlDown
                });
            }
        }

        public void HandleRequest(ItemRequestPacket packet)
        {
            if (packet.itemCode != inventory[packet.slotID]?.Itemstack?.Collectible?.Code?.ToString())
            {
                Api.Logger.Debug("Correct item not found");
                return;
            }
            IPlayer byPlayer = Api.World.PlayerByUid(packet.playerUID);
            ItemStack targetStack = inventory[packet.slotID].Itemstack;
            CollectibleObject targetItem = targetStack.Collectible;
            int targetCount = GameMath.Min(targetStack.StackSize, targetItem.MaxStackSize);
            if (packet.ctrl)
                targetCount = 1;
            int itemsMoved = 0;

            List<NetworkBlockEntity> connected = connectedNetwork.GetConnected();
            foreach(NetworkBlockEntity nbe in connected)
            {
                BETransferPlanet next = nbe as BETransferPlanet;
                if (next == null || next.extract || next.getConnectedInventory() == null || next.getConnectedInventory().Empty)
                    continue;

                foreach (ItemSlot fromSlot in next.getConnectedInventory())
                {
                    if (fromSlot.Itemstack != null && targetItem.Equals(targetStack, fromSlot.Itemstack, GlobalConstants.IgnoredStackAttributes))
                    {
                        ItemSlot toSlot = byPlayer.InventoryManager.GetBestSuitedSlot(fromSlot, true);
                        int temp = fromSlot.TryPutInto(Api.World, toSlot, targetCount-itemsMoved);
                        itemsMoved += temp;
                        if (temp > 0)
                        {
                            fromSlot.MarkDirty();
                            toSlot.MarkDirty();
                            next.MarkConnectedDirty();
                        }
                        if (itemsMoved >= targetCount)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public void BuildInventory()
        {
            List<ItemStack> stacks = new List<ItemStack>();
            HashSet<InventoryBase> invs = new HashSet<InventoryBase>();
            if (connectedNetwork != null)
            {
                foreach (NetworkBlockEntity nbe in connectedNetwork.GetConnected())
                {
                    BETransferPlanet betp = nbe as BETransferPlanet;
                    if (betp != null && !betp.extract && betp.getConnectedInventory() != null && !betp.getConnectedInventory().Empty && invs.Add(betp.getConnectedInventory()))
                    {
                        foreach (ItemSlot fromSlot in betp.getConnectedInventory())
                        {
                            if (fromSlot == null || fromSlot.Itemstack == null || !fromSlot.CanTake())
                                continue;

                            bool found = false;
                            foreach (ItemStack toStack in stacks)
                            {
                                if (fromSlot.Itemstack.Collectible.Equals(fromSlot.Itemstack, toStack, GlobalConstants.IgnoredStackAttributes))
                                {
                                    toStack.StackSize += fromSlot.Itemstack.StackSize;
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                stacks.Add(fromSlot.Itemstack.Clone());
                            }
                        }
                    }
                }
            }
            stacks.Sort(Comparer<ItemStack>.Create((a, b) => (b.StackSize - a.StackSize == 0 ? b.Id - a.Id : b.StackSize - a.StackSize)));

            if (inventory == null){
                inventory = new LookingGlassInventory("lginv-" + Pos.X + "/" + Pos.Y + "/" + Pos.Z, Api, stacks.Count, this);
            }
            else{
                inventory.Reset(stacks.Count);
            }
            for (int i = 0; i < stacks.Count; ++i)
            {
                inventory[i].Itemstack = stacks[i];
                inventory.MarkSlotDirty(i);
            }
        }

        public void ReloadGui()
        {
            BuildInventory();
            MarkDirty();
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            if (Api is ICoreClientAPI && gui != null && gui.IsOpened())
            {
                BuildInventory();
                gui.Recompose();
            }
        }
    }
}