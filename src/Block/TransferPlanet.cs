using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace cosmosis
{
    public class TransferPlanet : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            base.OnBlockInteractStart(world, byPlayer, blockSel);

            // Check for valid transfer planet
            BETransferPlanet betp = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BETransferPlanet;
            if(betp == null)
                return false;

            // Checks if the player is holding an item
            ItemStack heldStack = byPlayer.Entity.RightHandItemSlot.Itemstack;

            if (byPlayer.Entity.Controls.CtrlKey && (heldStack == null || (heldStack.Block != null && heldStack.Block.DrawType == EnumDrawType.Cube)))
            {
                betp.SetFacade(byPlayer.Entity.RightHandItemSlot);
                return true;
            }

            if (heldStack == null)
                return false;


            // Handle wrench code
            if (heldStack.Collectible is Wrench)
            {
                (heldStack.Collectible as Wrench).transferPlanetInteract(byPlayer, betp);
            }
            //Add or remove from filter
            else
            {
                // Remove from filter if sneaking
                if (byPlayer.Entity.Controls.Sneak && betp.filter.Contains(heldStack.Collectible.Code.ToString()))
                {
                    betp.filter.Remove(heldStack.Collectible.Code.ToString());
                    IServerPlayer sPlayer = byPlayer as IServerPlayer;
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, heldStack.GetName() + " removed from filter", EnumChatType.Notification);
                }

                // Add to filter if not sneaking
                else if (!byPlayer.Entity.Controls.Sneak && !betp.filter.Contains(heldStack.Collectible.Code.ToString()))
                {
                    betp.filter.Add(heldStack.Collectible.Code.ToString());
                    IServerPlayer sPlayer = byPlayer as IServerPlayer;
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, heldStack.GetName() + " added to filter", EnumChatType.Notification);
                }
            }

            return true;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            // Sets connection direction on block placement
            if(base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode))
            {
                BETransferPlanet betp = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BETransferPlanet;
                betp.SetConnectedInventory(blockSel.Position.Copy().Offset(blockSel.Face.Opposite));
                betp.channel = world.BlockAccessor.GetBlock(blockSel.Position).Variant["rock"];
                return true;
            }
            return false;
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BETransferPlanet betp = blockAccessor.GetBlockEntity(pos) as BETransferPlanet;
            if (betp != null)
                return betp.currentBox;
            else
                return base.GetSelectionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BETransferPlanet betp = blockAccessor.GetBlockEntity(pos) as BETransferPlanet;
            if (betp != null)
                return betp.currentBox;
            else
                return base.GetCollisionBoxes(blockAccessor, pos);
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            // Checks for valid transfer planet
            BETransferPlanet betp = world.BlockAccessor.GetBlockEntity(pos) as BETransferPlanet;
            if (betp == null)
                return null;

            // Gets the mode and filter items
            string info = (betp.extract ? "Extract mode" : "Insert mode");
            info += " - Priority: " + betp.priority;
            foreach (string item in betp.filter)
            {
                info += "\n" + new AssetLocation(item).GetName();
            }
            return info;
        }
    }
}
