using System;
using System.Collections.Generic;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace cosmosis
{
    public class BETransferPlanet : BlockEntity
    {

        public bool extract = false; // If this planet should insert or extract
        public int range = 5; // Search radius for other planets
        public int stackMoveSize = 4; // Items to move per transfer

        public string channel = ""; // Channel to interact with

        public BlockPos connectedTo; // Position of connected inventory

        public List<string> filter; // List of block/item id's to filter on

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
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
            if (!extract || inv == null)
                return;


            int itemsMoved = 0;
            // Check each item slot in inventory
            foreach (ItemSlot fromSlot in inv)
            {
                // Continue only if the item exists, can be taken, and is in the filter
                if (fromSlot != null && !fromSlot.Empty && fromSlot.CanTake() && ItemInFilter(fromSlot.Itemstack.Collectible.Code.ToString()))
                {
                    // Start BFS search for recieving inventory
                    List<BlockPos> visited = new List<BlockPos>();
                    Queue<BETransferPlanet> toProcess = new Queue<BETransferPlanet>();
                    FindConnections(Pos, visited, toProcess);

                    // Loop through all connected planets
                    while (toProcess.Count > 0)
                    {
                        // Process a planet and enqueue its neighbors
                        BETransferPlanet next = toProcess.Dequeue();
                        FindConnections(next.Pos, visited, toProcess);

                        // Skip planet if channels don't match
                        if(next.channel != channel)
                            continue;

                        // Skip this planet if it cannot insert into a valid inventory
                        if (next.extract || next.getConnectedInventory() == null || !next.ItemInFilter(fromSlot.Itemstack.Collectible.Code.ToString()))
                            continue;

                        // Find a recieving slot and transfer the items
                        ItemSlot toSlot = next.getConnectedInventory().GetAutoPushIntoSlot(BlockFacing.UP, fromSlot);
                        if (toSlot != null && toSlot.CanHold(fromSlot)) {
                            
                            // Move items
                            int temp = fromSlot.TryPutInto(Api.World, toSlot, Math.Min(fromSlot.StackSize, stackMoveSize));
                            itemsMoved += temp;
                            if (temp > 0){
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

        //Searches for connecting planets in a radius
        // current - position to seach around
        // visited - already found planets to ignore
        // toProcess - Newly found planets to process
        public void FindConnections(BlockPos current, List<BlockPos> visited, Queue<BETransferPlanet> toProcess)
        {
            // Search area
            for(int i = -range; i <= range; i++)
            {
                for(int j = -range; j <= range; j++)
                {
                    for(int k = -range; k <= range; k++)
                    {
                        // Checks for unvisited position
                        BlockPos check = current.AddCopy(i, j, k);
                        if (visited.Contains(check))
                            continue;

                        // Checks there is another valid transfer planet
                        BETransferPlanet connection = Api.World.BlockAccessor.GetBlockEntity(check) as BETransferPlanet;
                        if (connection == null || connection == this)
                            continue;

                        toProcess.Enqueue(connection);
                        visited.Add(check);
                    }
                }
            }
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
    }
}
