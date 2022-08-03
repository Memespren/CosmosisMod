using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Server;

[
    assembly: ModInfo(
        "Cosmosis", "cosmosis",
        Description = "Your friendly neighborhood cosmic storage mod",
        Authors = new []{"Memespren"},
        Version = "0.3.0")
]

namespace cosmosis
{
    public class CosmosisMod : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("transferplanet", typeof(TransferPlanet));
            api.RegisterBlockClass("shardcluster", typeof(ShardCluster));
            api.RegisterItemClass("wrench", typeof(Wrench));
            api.RegisterBlockEntityClass("transferplanet", typeof(BETransferPlanet));
        }
    }
}
