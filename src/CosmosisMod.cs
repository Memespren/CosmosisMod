using System;
using Vintagestory.API;
using Vintagestory.API.Common;

[
    assembly: ModInfo(
        "Cosmosis", "cosmosis",
        Description = "Your friendly neighborhood cosmic storage mod",
        Authors = new []{"Memespren"},
        Version = "0.1.0")
]

namespace cosmosis
{
    public class CosmosisMod : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockEntityClass("transferplanet", typeof(BETransferPlanet));
            api.RegisterBlockClass("transferplanet", typeof(TransferPlanet));
            api.RegisterBlockClass("shardcluster", typeof(ShardCluster));
            api.RegisterItemClass("wrench", typeof(Wrench));
        }
    }
}
