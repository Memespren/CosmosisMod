using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.MathTools;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace cosmosis
{
    public class Wrench : Item
    {
        SkillItem[] toolModes; // Tool modes for wrench

        List<int> netColor = new List<int>(); // {34, 7, 24};
        List<int> invColor = new List<int>(); // {7, 7, 27};

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            netColor.Add(ColorUtil.ColorFromRgba(0, 255, 255, 50));
            invColor.Add(ColorUtil.ColorFromRgba(255, 165, 0, 50));


            // Setup tool modes on client
            ICoreClientAPI client = api as ICoreClientAPI;
            if(client == null)
                return;

            toolModes = new SkillItem[]
            {
                new SkillItem()
                {
                    Code = new AssetLocation("switchmode"),
                    Name = "Toggle Mode"
                }.WithLetterIcon(client, "T"),
                new SkillItem()
                {
                    Code = new AssetLocation("clearfilter"),
                    Name = "Clear Filter"
                }.WithLetterIcon(client, "C"),
                new SkillItem()
                {
                    Code = new AssetLocation("snapshot"),
                    Name = "Snapshot"
                }.WithLetterIcon(client, "S"),
                new SkillItem()
                {
                    Code = new AssetLocation("moveconnected"),
                    Name = "Move Connected Tile"
                }.WithLetterIcon(client, "M"),
                new SkillItem()
                {
                    Code = new AssetLocation("changepriority"),
                    Name = "Change Priority"
                }.WithLetterIcon(client, "P")
            };
        }

        public override void OnUnloaded(ICoreAPI api)
        {
            // Dispose of tool modes when unloaded
            if (toolModes != null){
                for(int i = 0; i < toolModes.Length; ++i)
                {
                    if (toolModes[i] != null)
                        toolModes[i].Dispose();
                }
            }
        }


        public void transferPlanetInteract(IPlayer player, BETransferPlanet betp)
        {
            IServerPlayer sPlayer = player as IServerPlayer;

            //Switch on tool modes
            switch(player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetInt("toolMode", 0))
            {
                case 0: // Toggle modes
                    betp.extract = !betp.extract;
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, (betp.extract ? "Extract" : "Insert") + " mode set", EnumChatType.Notification);
                    break;
                
                case 1: // Clear filter
                    betp.filter.Clear();
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, "Filter cleared", EnumChatType.Notification);
                    break;

                case 2: // Take snapshot
                    if (betp.getConnectedInventory() == null)
                        break;

                    foreach(ItemSlot invSlot in betp.getConnectedInventory())
                    {
                        if (!invSlot.Empty && !betp.filter.Contains(invSlot.Itemstack.Collectible.Code.ToString()))
                            betp.filter.Add(invSlot.Itemstack.Collectible.Code.ToString());
                    }
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, "Contents added to filter", EnumChatType.Notification);
                    break;

                case 3: // Change connection
                    betp.SetConnectedInventory(betp.Pos.Copy().Offset(player.CurrentBlockSelection.Face.Opposite));
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, "Moved connection", EnumChatType.Notification);
                    break;

                case 4: // Change Priority
                    if (player.Entity.Controls.Sneak)
                        --betp.priority;
                    else
                        ++betp.priority;
                    betp.connectedNetwork.Resort();
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, "Priority changed to " + betp.priority, EnumChatType.Notification);
                    break;
            }
            betp.MarkDirty();
        }

         public void liquidPlanetInteract(IPlayer player, BELiquidPlanet belp)
        {
            IServerPlayer sPlayer = player as IServerPlayer;

            //Switch on tool modes
            switch(player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetInt("toolMode", 0))
            {
                case 0: // Toggle modes
                    belp.extract = !belp.extract;
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, (belp.extract ? "Extract" : "Insert") + " mode set", EnumChatType.Notification);
                    break;
                
                case 1: // Clear filter
                    belp.filter.Clear();
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, "Filter cleared", EnumChatType.Notification);
                    break;

                case 2: // Take snapshot
                    ILiquidInterface container = belp.GetLiquidContainer();
                    if (container != null)
                    {
                        if (container.GetContent(belp.connectedTo) != null)
                        {
                            string entry = container.GetContent(belp.connectedTo).Collectible.Code.ToString();
                            if (!belp.filter.Contains(entry))
                                belp.filter.Add(entry);
                        }
                    }
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, "Contents added to filter", EnumChatType.Notification);
                    break;

                case 3: // Change connection
                    belp.SetConnectedInventory(belp.Pos.Copy().Offset(player.CurrentBlockSelection.Face.Opposite));
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, "Moved connection", EnumChatType.Notification);
                    break;

                case 4: // Change Priority
                    if (player.Entity.Controls.Sneak)
                        --belp.priority;
                    else
                        ++belp.priority;
                    belp.connectedNetwork.Resort();
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, "Priority changed to " + belp.priority, EnumChatType.Notification);
                    break;
            }
            belp.MarkDirty();
        }

        public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
        {
            return toolModes;
        }

        public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection)
        {
            return slot.Itemstack.Attributes.GetInt("toolMode", 0);
        }

        public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, int toolMode)
        {
            slot.Itemstack.Attributes.SetInt("toolMode", toolMode);
        }

        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            // Check for valid player
            EntityPlayer ePlayer = byEntity as EntityPlayer;
            if (ePlayer == null)
                return;

            // Clear highlight list
            IPlayer player = ePlayer.Player;


            List<BlockPos> inventoryList = new List<BlockPos>();
            List<BlockPos> networkList = new List<BlockPos>();
            if (player.CurrentBlockSelection != null)
            {
                // Add selected transfer planet to highlight list
                BETransferPlanet betp = api.World.BlockAccessor.GetBlockEntity(player.CurrentBlockSelection.Position) as BETransferPlanet;
                if (betp != null){
                    inventoryList.Add(betp.connectedTo);
                    foreach (NetworkBlockEntity nbe in betp.connectedNetwork.GetConnected())
                    {
                        networkList.Add(nbe.Pos);
                    }
                }

                BELiquidPlanet belp = api.World.BlockAccessor.GetBlockEntity(player.CurrentBlockSelection.Position) as BELiquidPlanet;
                if (belp != null)
                {
                    inventoryList.Add(belp.connectedTo);
                    foreach(NetworkBlockEntity nbe in belp.connectedNetwork.GetConnected())
                    {
                        networkList.Add(nbe.Pos);
                    }
                }

                EnergyBlockEntity ebe = api.World.BlockAccessor.GetBlockEntity(player.CurrentBlockSelection.Position) as EnergyBlockEntity;
                if (ebe != null)
                {
                    foreach(EnergyBlockEntity neighbor in ebe.neighbors)
                    {
                        networkList.Add(neighbor.Pos);
                    }
                }
            }
            // Update highlights
            api.World.HighlightBlocks(player, 51, inventoryList, invColor);
            api.World.HighlightBlocks(player, 52, networkList, netColor);
        }
    }
}