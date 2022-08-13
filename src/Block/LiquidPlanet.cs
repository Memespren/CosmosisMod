using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace cosmosis
{
    public class LiquidPlanet : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            base.OnBlockInteractStart(world, byPlayer, blockSel);

            // Check for valid liquid planet
            BELiquidPlanet belp = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELiquidPlanet;
            if(belp == null)
                return false;

            // Checks if the player is holding an item
            ItemStack heldStack = byPlayer.Entity.RightHandItemSlot.Itemstack;

            if (byPlayer.Entity.Controls.CtrlKey && (heldStack == null || (heldStack.Block != null && heldStack.Block.DrawType == EnumDrawType.Cube)))
            {
                belp.SetFacade(byPlayer.Entity.RightHandItemSlot);
                return true;
            }

            if (heldStack == null)
                return false;


            // Handle wrench code
            if (heldStack.Collectible is Wrench)
            {
                (heldStack.Collectible as Wrench).liquidPlanetInteract(byPlayer, belp);
            }
            //Add or remove from filter
            else if (heldStack.Collectible is ILiquidInterface)
            {
                ILiquidInterface container = heldStack.Collectible as ILiquidInterface;
                if (container.GetContent(heldStack) == null)
                    return false;
                // Remove from filter if sneaking
                if (byPlayer.Entity.Controls.Sneak && belp.filter.Contains(container.GetContent(heldStack).Collectible.Code.ToString()))
                {
                    belp.filter.Remove(container.GetContent(heldStack).Collectible.Code.ToString());
                    IServerPlayer sPlayer = byPlayer as IServerPlayer;
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, container.GetContent(heldStack).GetName() + " removed from filter", EnumChatType.Notification);
                }

                // Add to filter if not sneaking
                else if (!byPlayer.Entity.Controls.Sneak && !belp.filter.Contains(container.GetContent(heldStack).Collectible.Code.ToString()))
                {
                    belp.filter.Add(container.GetContent(heldStack).Collectible.Code.ToString());
                    IServerPlayer sPlayer = byPlayer as IServerPlayer;
                    if (sPlayer != null)
                        sPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, container.GetContent(heldStack).GetName() + " added to filter", EnumChatType.Notification);
                }
                return true;
            }

            return true;
        }

        public override bool TryPlaceBlock(IWorldAccessor world, IPlayer byPlayer, ItemStack itemstack, BlockSelection blockSel, ref string failureCode)
        {
            // Sets connection direction on block placement
            if(base.TryPlaceBlock(world, byPlayer, itemstack, blockSel, ref failureCode))
            {
                BELiquidPlanet belp = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELiquidPlanet;
                belp.SetConnectedInventory(blockSel.Position.Copy().Offset(blockSel.Face.Opposite));
                belp.channel = world.BlockAccessor.GetBlock(blockSel.Position).Variant["rock"];
                return true;
            }
            return false;
        }

        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BELiquidPlanet belp = blockAccessor.GetBlockEntity(pos) as BELiquidPlanet;
            if (belp != null)
                return belp.currentBox;
            else
                return base.GetSelectionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BELiquidPlanet belp = blockAccessor.GetBlockEntity(pos) as BELiquidPlanet;
            if (belp != null)
                return belp.currentBox;
            else
                return base.GetCollisionBoxes(blockAccessor, pos);
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            // Checks for valid transfer planet
            BELiquidPlanet belp = world.BlockAccessor.GetBlockEntity(pos) as BELiquidPlanet;
            if (belp == null)
                return null;

            // Gets the mode and filter items
            string info = (belp.extract ? "Extract mode" : "Insert mode");
            info += " - Priority: " + belp.priority;
            foreach (string item in belp.filter)
            {
                info += "\n" + new AssetLocation(item).GetName();
            }
            return info;
        }
    }
}
