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
    public class BETransferPlanet : NetworkBlockEntity, IFacadable
    {

        public bool extract = false; // If this planet should insert or extract

        public int stackMoveSize = 4; // Items to move per transfer

        public string channel = ""; // Channel to interact with

        public BlockPos connectedTo; // Position of connected inventory

        public List<string> filter; // List of block/item id's to filter on

        private TransferPlanetRenderer renderer; // Rendering handler 

        private FacadeInventory facadeInv; // Inventory for storing facade block

        private MeshData facadeMesh = null; // Mesh for facade rendering

        private Cuboidf[] smallBox; // Reference to the original hitbox
        private Cuboidf[] bigBox; // Reference to the large hitbox

        public Cuboidf[] currentBox; // The current hitbox to use


        // Create inventory
        public BETransferPlanet()
        {
            facadeInv = new FacadeInventory(null, null);
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            //Initialize facade inventory
            facadeInv.LateInitialize("tpfacade-" + Pos.X + "/" + Pos.Y + "/" + Pos.Z, api);

            smallBox = Block.CollisionBoxes;
            bigBox = new Cuboidf[]{Block.DefaultCollisionBox};
            currentBox = smallBox;

            //Handle client initialization
            if (api.Side == EnumAppSide.Client)
            {
                //Generate planet mesh for renderer
                if (baseMesh == null){
                    Block block = api.World.BlockAccessor.GetBlock(Pos);
                    if (block.BlockId != 0)
                    {
                        MeshData mesh;
                        ITesselatorAPI mesher = (api as ICoreClientAPI).Tesselator;
                        mesher.TesselateShape(block, Shape.TryGet(Api, "cosmosis:shapes/block/planet.json"), out mesh);
                        baseMesh = mesh;
                    }
                }
                //Register renderer
                renderer = new TransferPlanetRenderer(api as ICoreClientAPI, Pos, baseMesh);
                (api as ICoreClientAPI).Event.RegisterRenderer(renderer, EnumRenderStage.Opaque, "transferplanet");
                (api as ICoreClientAPI).Event.AfterActiveSlotChanged += OnSlotChanged;
            }
            ApplyFacade();

            if (connectedTo != null)
                SetConnectedInventory(connectedTo);

            RegisterGameTickListener(OnGameTick, 1000);

            if (filter == null)
                filter = new List<string>();

        }

        public void OnSlotChanged(ActiveSlotChangeEventArgs args)
        {
            if (Api is ICoreClientAPI)
            {
                ItemStack selected = (Api as ICoreClientAPI).World.Player.InventoryManager.ActiveHotbarSlot.Itemstack;
                if (selected == null || (selected.Collectible as Wrench) == null)
                {
                    ShowFacade();
                }
                else
                {
                    HideFacade();
                }
            }
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
                                MarkConnectedDirty();
                                next.MarkConnectedDirty();
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

        public void SetFacade(ItemSlot fromSlot)
        {
            if (facadeInv[0].Itemstack != null)
                Api.World.SpawnItemEntity(facadeInv[0].TakeOut(1), Pos.ToVec3d());

            if (fromSlot.Itemstack != null)
                facadeInv[0].Itemstack = fromSlot.TakeOut(1);

            ApplyFacade();
            fromSlot.MarkDirty();
            facadeInv[0].MarkDirty();
        }

        public void ApplyFacade()
        {
            if (facadeInv[0].Itemstack != null)
            {
                if (Api.Side == EnumAppSide.Client)
                {
                    //Generate Mesh
                    MeshData mesh;
                    ITesselatorAPI mesher = (Api as ICoreClientAPI).Tesselator;
                    mesher.TesselateShape("facade", Shape.TryGet(Api, "game:shapes/block/basic/cube.json"), out mesh, mesher.GetTexSource(facadeInv[0].Itemstack.Block));
                    facadeMesh = mesh;
                    renderer.doRender = false;
                }
                currentBox = bigBox;
            }
            else
            {
                if (Api.Side == EnumAppSide.Client)
                {
                    facadeMesh = null;
                    renderer.doRender = true;
                }
                currentBox = smallBox;
            }

            Api.World.BlockAccessor.MarkBlockDirty(Pos);
        }

        public void HideFacade()
        {
            renderer.doRender = true;
            MarkDirty(true);
        }

        public void ShowFacade()
        {
            renderer.doRender = (facadeMesh == null);
            MarkDirty(true);
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (Block == null)
                return false;

            if (facadeMesh != null && !renderer.doRender){
                mesher.AddMeshData(facadeMesh.Clone());
            }
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

        public void SetConnectedInventory(BlockPos pos)
        {
            InventoryBase inv;
            if (connectedTo != null)
            {
                inv = getConnectedInventory();
                if (inv != null)
                    inv.SlotModified -= OnContainerChanged;
            }
            connectedTo = pos;
            inv = getConnectedInventory();
            if (inv != null)
                inv.SlotModified += OnContainerChanged;
        }

        public void MarkConnectedDirty()
        {
             BlockEntityContainer container = Api.World.BlockAccessor.GetBlockEntity(connectedTo) as BlockEntityContainer;
            if (container != null)
                container.MarkDirty();
        }

        public void OnContainerChanged(int index)
        {
            if (connectedNetwork != null)
                RegisterDelayedCallback(connectedNetwork.OnInventoryChanged, 100);
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
            ITreeAttribute facadeTree = new TreeAttribute();
            facadeInv.ToTreeAttributes(facadeTree);
            tree["facade"] = facadeTree;
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
            facadeInv.FromTreeAttributes(tree.GetTreeAttribute("facade"));
            if (Api != null)
            {
                facadeInv.AfterBlocksLoaded(Api.World);
            }
        }

        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            base.OnBlockBroken(byPlayer);
            facadeInv.DropAll(Pos.ToVec3d());
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

        ~BETransferPlanet()
        {
            if (Api is ICoreClientAPI)
            {
                (Api as ICoreClientAPI).Event.AfterActiveSlotChanged -= OnSlotChanged;
            }

            if (getConnectedInventory() != null)
            {
                getConnectedInventory().SlotModified -= OnContainerChanged;
            }
        }
    }
}
