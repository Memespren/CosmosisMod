using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;


namespace cosmosis
{
    public class LookingGlass : Block
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BELookingGlass belg = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BELookingGlass;
            if (belg != null)
            {
                if (byPlayer.Entity.Controls.Sneak)
                {
                    belg.BuildInventory();
                }
                else{
                    belg.OpenGUI();
                }
            }

            return true;
        }
    }
}