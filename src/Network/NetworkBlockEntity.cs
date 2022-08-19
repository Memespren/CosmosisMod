using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class NetworkBlockEntity : BlockEntity, IHighlightable
    {

        public Network connectedNetwork; // Shared network for connected entities

        public int range = 5; // Search radius for other network entities

        public int priority = 0; // Priority over other entities

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            FindNetwork();
        }

        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();
            connectedNetwork.RemoveAndReclaculate(this);
        }

        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            connectedNetwork.RemoveAndReclaculate(this);
        }

        // Gets a list of all NBEs in range
        public List<NetworkBlockEntity> GetNeighbors()
        {
            List<NetworkBlockEntity> neighbors = new List<NetworkBlockEntity>();
            for(int i = -range; i <= range; i++)
            {
                for(int j = -range; j <= range; j++)
                {
                    for(int k = -range; k <= range; k++)
                    {
                        BlockPos check = Pos.AddCopy(i, j, k);
                        NetworkBlockEntity nbe = Api.World.BlockAccessor.GetBlockEntity(check) as NetworkBlockEntity;
                        if (nbe == null || nbe == this)
                            continue;

                        neighbors.Add(nbe);
                    }
                }
            }
            return neighbors;
        }

        // Connects to a network
        // Joins a network if found
        // Merges multiple networks if found
        // Creates a new network if needed
        public void FindNetwork()
        {
            foreach(NetworkBlockEntity nbe in GetNeighbors())
            {
                if(nbe.connectedNetwork != null)
                {
                    if (connectedNetwork == null)
                    {
                        //Api.Logger.Debug("Joining network with " + nbe.connectedNetwork.GetConnected().Count + " entities");
                        nbe.connectedNetwork.Add(this);
                    }
                    else if (connectedNetwork != nbe.connectedNetwork)
                    {
                        //Api.Logger.Debug("Merging network with " + nbe.connectedNetwork.GetConnected().Count + " entities");
                        connectedNetwork.Merge(nbe.connectedNetwork);
                    }
                }
            }
            
            if (connectedNetwork == null)
            {
                //Api.Logger.Debug("Creating new network");
                Network net = new Network(this);
            }
        }

        public virtual void ConnectToNetwork(Network net)
        {
            connectedNetwork = net;
        }

        public virtual void GetHighlightedBlocks(ref List<BlockPos> blueList, ref List<BlockPos> orangeList)
        {
            foreach(NetworkBlockEntity nbe in connectedNetwork.GetConnected())
            {
                blueList.Add(nbe.Pos);
            }
        }

        // Saves attributes to the tree
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("priority", priority);
        }

        // Loads attributes from the tree
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            priority = tree.GetInt("priority", priority);
        }
    }
}
