using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace cosmosis
{
    public class BETransferPlanet : NetworkBlockEntity
    {

        public bool extract = false; // If this planet should insert or extract

        public int stackMoveSize = 4; // Items to move per transfer

        public string channel = ""; // Channel to interact with

        public BlockPos connectedTo; // Position of connected inventory

        public List<string> filter; // List of block/item id's to filter on

        public TransferPlanetRenderer renderer; // Rendering handler

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api.Side == EnumAppSide.Client)
            {
                //Generate Mesh
                if (baseMesh == null){
                    Block block = api.World.BlockAccessor.GetSolidBlock(Pos);
                    if (block.BlockId != 0)
                    {
                        MeshData mesh;
                        ITesselatorAPI mesher = ((ICoreClientAPI)api).Tesselator;
                        mesher.TesselateShape(block, Shape.TryGet(Api, "cosmosis:shapes/block/planet.json"), out mesh);
                        baseMesh = mesh;
                    }
                }
                //Register Renderer
                renderer = new TransferPlanetRenderer(api as ICoreClientAPI, Pos, baseMesh);
                (api as ICoreClientAPI).Event.RegisterRenderer(renderer, EnumRenderStage.Opaque, "transferplanet");
            }
            RegisterGameTickListener(OnGameTick, 1000);
            if (filter == null)
                filter = new List<string>();
        }
        public void OnGameTick(float dt)
        {
            // Ensure the transfer only occurs on the server
            if (!(Api is ICoreServerAPI))
                return;

            // Only process if it can extract from a valid inventory
            InventoryBase inv = getConnectedInventory();
            if (!extract || inv == null || inv.Empty || connectedNetwork == null)
                return;

            
            //Get list of entities on network
            List<NetworkBlockEntity> connected = connectedNetwork.GetConnected();
            int itemsMoved = 0;

            // Check each item slot in inventory
            foreach (ItemSlot fromSlot in inv)
            {
                // Continue only if the item exists, can be taken, and is in the filter
                if (fromSlot != null && !fromSlot.Empty && fromSlot.CanTake() && ItemInFilter(fromSlot.Itemstack.Collectible.Code.ToString()))
                {
                    // Loop through all connected entities
                    foreach(NetworkBlockEntity nbe in connected)
                    {
                        // Check if it is a transfer planet on the same network
                        BETransferPlanet next = nbe as BETransferPlanet;
                        if (next == null || next.channel != channel)
                            continue;

                        // Skip this planet if it cannot insert into a valid inventory
                        if (next.extract || next.getConnectedInventory() == null || !next.ItemInFilter(fromSlot.Itemstack.Collectible.Code.ToString()))
                            continue;

                        // Find a recieving slot and transfer the items
                        ItemSlot toSlot = next.getConnectedInventory().GetAutoPushIntoSlot(BlockFacing.UP, fromSlot);
                        if (toSlot != null && toSlot.CanHold(fromSlot))
                        {    
                            // Move items
                            int temp = fromSlot.TryPutInto(Api.World, toSlot, Math.Min(fromSlot.StackSize, stackMoveSize));
                            itemsMoved += temp;
                            if (temp > 0)
                            {
                                fromSlot.MarkDirty();
                                toSlot.MarkDirty();
                            }

                            // End the transfer if the stack is empty or the max items are moved
                            if (fromSlot.StackSize <= 0 || itemsMoved >= stackMoveSize)
                                return;
                        }
                    }
                }
            }
        }

        MeshData baseMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("cosmosis:transferplanet-" + Block.Variant["rock"], out value);
                return (MeshData)value;
            }
            set
            {
                Api.ObjectCache["cosmosis:transferplanet-" + Block.Variant["rock"]] = value;
            }
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (Block == null)
                return false;

            // mesher.AddMeshData(this.baseMesh.Clone()
            // .Translate(0, yoffset, 0)
            // .Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0, angle*GameMath.DEG2RAD, 0));
            return true;
        }

        // Gets the inventory this planet interfaces with
        public InventoryBase getConnectedInventory()
        {
            BlockEntityContainer container = Api.World.BlockAccessor.GetBlockEntity(connectedTo) as BlockEntityContainer;
            if (container == null)
                return null;
            else
                return container.Inventory;
        }

        // Checks if an item can be processed
        public bool ItemInFilter(string code)
        {
            return (filter.Count == 0 || filter.Contains(code));
        }

        // Saves attributes to the tree
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetBlockPos("connectedTo", connectedTo);
            tree.SetBool("extract", extract);
            tree.SetString("channel", channel);
            tree.SetInt("filterSize", filter.Count);
            for(int i = 0; i < filter.Count; i++)
            {
                tree.SetString("filter" + i, filter[i]);
            }
        }

        // Loads attributes from the tree
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            connectedTo = tree.GetBlockPos("connectedTo");
            extract = tree.GetBool("extract");
            channel = tree.GetString("channel");
            filter = new List<string>();
            int filterSize = tree.GetInt("filterSize");
            for(int i = 0; i < filterSize; ++i)
            {
                filter.Add(tree.GetString("filter" + i));
            }
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            renderer?.Dispose();
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            renderer?.Dispose();
        }
    }
}
