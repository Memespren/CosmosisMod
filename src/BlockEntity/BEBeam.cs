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

            //RegisterGameTickListener(OnGameTick, 1000);
        }

        public void OnGameTick(float dt)
        {
            if (renderer != null && player != null)
            {   
                renderer.Shoot();
                renderer.setTarget(player.CameraPos);
            }   
        }

        public override void AlignTo(BlockPos pos)
        {
           renderer?.setTarget(pos);
        }

        public override void Trigger()
        {
            renderer?.Shoot();
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
