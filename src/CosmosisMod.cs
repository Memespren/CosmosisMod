﻿using ProtoBuf;
using System.IO;
using Vintagestory.API.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.MathTools;
using Vintagestory.API.Datastructures;

[
    assembly: ModInfo(
        "Cosmosis", "cosmosis",
        Description = "Your friendly neighborhood cosmic storage mod",
        Authors = new []{"Memespren"},
        Version = "0.5.1")
]

namespace cosmosis
{
    public class CosmosisMod : ModSystem
    {
        public ICoreClientAPI capi;
        public ICoreServerAPI sapi;
        public static IServerNetworkChannel requestServerChannel;
        public static IClientNetworkChannel requestClientChannel;
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("transferplanet", typeof(TransferPlanet));
            api.RegisterBlockClass("shardcluster", typeof(ShardCluster));
            api.RegisterBlockClass("lookingglass", typeof(LookingGlass));
            api.RegisterBlockClass("liquidplanet", typeof(LiquidPlanet));
            api.RegisterItemClass("wrench", typeof(Wrench));
            api.RegisterBlockEntityClass("transferplanet", typeof(BETransferPlanet));
            api.RegisterBlockEntityClass("lookingglass", typeof(BELookingGlass));
            api.RegisterBlockEntityClass("liquidplanet", typeof(BELiquidPlanet));
            api.RegisterBlockEntityClass("beam", typeof(BEBeam));
            api.RegisterBlockEntityClass("bigbeam", typeof(BEBigBeam));
            api.RegisterBlockEntityClass("sunholder", typeof (BESunHolder));
            api.RegisterBlockEntityClass("esource", typeof(EnergySource));
            api.RegisterBlockEntityClass("esink", typeof(EnergySink));

            api.Network.RegisterChannel("cosmosis:itemrequest").RegisterMessageType(typeof(ItemRequestPacket));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;
            requestClientChannel = api.Network.GetChannel("cosmosis:itemrequest");
            requestClientChannel.SetMessageHandler<ItemRequestPacket>(OnItemRequest);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            sapi = api;
            requestServerChannel = api.Network.GetChannel("cosmosis:itemrequest");
            requestServerChannel.SetMessageHandler<ItemRequestPacket>(OnItemRequest);
        }

        public void OnItemRequest(IPlayer sender, ItemRequestPacket packet)
        {
            //requestServerChannel.BroadcastPacket<ItemRequestPacket>(packet);
            BlockPos pos = new BlockPos(packet.X, packet.Y, packet.Z);
            BELookingGlass belg = sapi.World.BlockAccessor.GetBlockEntity(pos) as BELookingGlass;
            if (belg != null)
            {
                belg.HandleRequest(packet);
            }
        }

        public void OnItemRequest(ItemRequestPacket packet)
        {
            BlockPos pos = new BlockPos(packet.X, packet.Y, packet.Z);
            BELookingGlass belg = capi.World.BlockAccessor.GetBlockEntity(pos) as BELookingGlass;
            if (belg != null)
            {
                belg.HandleRequest(packet);
            }
        }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ItemRequestPacket
    {
        public int X;
        public int Y;
        public int Z;

        public int slotID;
        public string itemCode;

        public string playerUID;

        public bool ctrl;

    }
}
