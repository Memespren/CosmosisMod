using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace cosmosis
{
    public class Network
    {
        public ICoreAPI api;
        public event Action ContentsModified;
        private List<NetworkBlockEntity> connected; // List of NBEs on the network

        // Creates a new network with one connected NBE
        public Network(NetworkBlockEntity first)
        {
            api = first.Api;
            connected = new List<NetworkBlockEntity>();
            Add(first);
        }

        // Adds a new NBE to the network
        public void Add(NetworkBlockEntity nbe)
        {
            if (nbe.connectedNetwork != this)
            {
                nbe.connectedNetwork = this;
                connected.Add(nbe);
                Resort();
            }
        }

        // Resorts the network based on priorities
        public void Resort()
        {
            connected.Sort(Comparer<NetworkBlockEntity>.Create((a, b) => b.priority - a.priority));
        }

        // Merges another network into this one
        public void Merge(Network toMerge)
        {
            foreach(NetworkBlockEntity nbe in toMerge.connected)
            {
                Add(nbe);
            }
        }

        // Recalculates network connections without given NBE
        public void RemoveAndReclaculate(NetworkBlockEntity toRemove)
        {
            foreach(NetworkBlockEntity nbe in connected)
            {
                nbe.connectedNetwork = null;
            }
            foreach(NetworkBlockEntity nbe in connected)
            {
                if (nbe != toRemove)
                    nbe.connectToNetwork();
            }
        }

        // Gets list of connected NBEs
        public List<NetworkBlockEntity> GetConnected()
        {
            return connected;
        }

        public void OnInventoryChanged(float dt)
        {
            ContentsModified?.Invoke();
        }
    }
}