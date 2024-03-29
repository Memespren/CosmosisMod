﻿using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace cosmosis
{
    public class BEBeam : EnergyNode
    {

        private BeamRenderer renderer; // Rendering handler 

        private EntityPlayer player;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            //Handle client initialization
            if (api.Side == EnumAppSide.Client)
            {
                player = (api as ICoreClientAPI).World.Player.Entity;
                //Generate planet mesh for renderer
                if (baseMesh == null){
                    Block block = api.World.BlockAccessor.GetBlock(Pos);
                    if (block.BlockId != 0)
                    {
                        MeshData mesh;
                        ITesselatorAPI mesher = (api as ICoreClientAPI).Tesselator;
                        mesher.TesselateShape(block, Shape.TryGet(Api, "cosmosis:shapes/block/beambase.json"), out mesh);
                        baseMesh = mesh;
                    }
                }
                //Generate planet mesh for renderer
                if (topMesh == null){
                    Block block = api.World.BlockAccessor.GetBlock(Pos);
                    if (block.BlockId != 0)
                    {
                        MeshData mesh;
                        ITesselatorAPI mesher = (api as ICoreClientAPI).Tesselator;
                        mesher.TesselateShape(block, Shape.TryGet(Api, "cosmosis:shapes/block/beamtop.json"), out mesh);
                        topMesh = mesh;
                    }
                }
                //Generate planet mesh for renderer
                if (shotMesh == null){
                    Block block = api.World.BlockAccessor.GetBlock(Pos);
                    if (block.BlockId != 0)
                    {
                        MeshData mesh;
                        ITesselatorAPI mesher = (api as ICoreClientAPI).Tesselator;
                        mesher.TesselateShape(block, Shape.TryGet(Api, "cosmosis:shapes/block/energybeam.json"), out mesh);
                        shotMesh = mesh;
                    }
                }
                //Register renderer
                renderer = new BeamRenderer(api as ICoreClientAPI, Pos, baseMesh, topMesh, shotMesh, Block.LastCodePart());
                (api as ICoreClientAPI).Event.RegisterRenderer(renderer, EnumRenderStage.Opaque, "beam");
            }
        }

        public override bool AlignTo(EnergyBlockEntity target)
        {
            renderer?.setTarget(target.Pos);

            BlockSelection bsel = null;
            EntitySelection esel = null;
            Api.World.RayTraceForSelection(Pos.ToVec3d().Add(0.5,0.5,0.5), target.Pos.ToVec3d().Add(0.5,0.5,0.5), ref bsel, ref esel,
            (p, block) => {
                return block.Attributes?["beamPassable"]?.Exists == false;
            },
            (entity) => false);

            if (bsel != null)
            {
                RemoveNeighbor(target);
                target.RemoveNeighbor(this);
                return false;
            }
            return true;
        }

        public override void Trigger()
        {
            renderer?.Shoot();
        }

        public override bool CheckNeighbor(EnergyBlockEntity other)
        {
            if (Api == null)
                return false;

            BlockSelection bsel = null;
            EntitySelection esel = null;
            Api.World.RayTraceForSelection(Pos.ToVec3d().Add(0.5,0.5,0.5), other.Pos.ToVec3d().Add(0.5,0.5,0.5), ref bsel, ref esel,
            (pos, block) => {
                return block.Attributes?["beamPassable"].Exists == false;
            },
            (entity) => false);

            if(bsel != null)
                return false;

            BEBeam otherBeam = other as BEBeam;
            if (otherBeam != null)
            {
                return Block.Variant["metal"] == otherBeam.Block.Variant["metal"];
            }
            return base.CheckNeighbor(other);

        }

        MeshData baseMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("cosmosis:beambase-" + Block.Variant["wood"] + "-" + Block.Variant["metal"], out value);
                return (MeshData)value;
            }
            set
            {
                Api.ObjectCache["cosmosis:beambase-" + Block.Variant["wood"] + "-" + Block.Variant["metal"]] = value;
            }
        }

        MeshData topMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("cosmosis:beamtop-" + Block.Variant["wood"] + "-" + Block.Variant["metal"], out value);
                return (MeshData)value;
            }
            set
            {
                Api.ObjectCache["cosmosis:beamtop-" + Block.Variant["wood"] + "-" + Block.Variant["metal"]] = value;
            }
        }

        MeshData shotMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("cosmosis:energybeam", out value);
                return (MeshData)value;
            }
            set
            {
                Api.ObjectCache["cosmosis:energybeam"] = value;
            }
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            return (renderer != null);
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
