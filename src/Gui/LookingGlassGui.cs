using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class LookingGlassGui : GuiDialogBlockEntityInventory
    {
        //LookingGlassInventory inventory;
        public LookingGlassGui(string dialogTitle, LookingGlassInventory inventory, BlockPos blockEntityPos, ICoreClientAPI capi)
        : base(dialogTitle, inventory, blockEntityPos, 8, capi)
        {
            if (this.IsDuplicate)
                return;
        }
    }
}