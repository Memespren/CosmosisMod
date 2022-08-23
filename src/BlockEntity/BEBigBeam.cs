using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace cosmosis
{
    public class BEBigBeam : EnergySink
    {

        private BigBeamRenderer renderer; // Rendering handler 

        private Entity target;

        public static HashSet<Entity> targets;

        static BEBigBeam()
        {
            targets = new HashSet<Entity>();
        }


        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

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
                        mesher.TesselateShape(block, Shape.TryGet(Api, "cosmosis:shapes/block/bigbase.json"), out mesh);
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
                        mesher.TesselateShape(block, Shape.TryGet(Api, "cosmosis:shapes/block/bigtop.json"), out mesh);
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
                renderer = new BigBeamRenderer(api as ICoreClientAPI, Pos, baseMesh, topMesh, shotMesh);
                (api as ICoreClientAPI).Event.RegisterRenderer(renderer, EnumRenderStage.Opaque, "bigbeam");
            }
            RegisterGameTickListener(OnGameTick, 100);
            //RegisterGameTickListener(GameTick, 1000);

            Setup(800, 200, 1000);
        }

        public void OnGameTick(float dt)
        {
            if (target != null && renderer != null)
                renderer.setTarget(target.Pos.XYZ);
        }

        public override bool DoOperation()
        {
            if (target != null)
            {
                (target as EntityAgent).ReceiveDamage(new DamageSource(), 100);
                targets.Remove(target);
                target = null;
                if (renderer != null)
                    renderer.Shoot();

                return true;
            }
            else
            {
                target = Api.World.GetNearestEntity(Pos.ToVec3d(), 50, 50, (entity) => ((entity is EntityDrifter) && entity.Alive && !targets.Contains(entity)));
                targets.Add(target);
                return false;
            }
        }

        MeshData baseMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("cosmosis:bigbeambase", out value);
                return (MeshData)value;
            }
            set
            {
                Api.ObjectCache["cosmosis:bigbeambase"] = value;
            }
        }

        MeshData topMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("cosmosis:bigbeamtop", out value);
                return (MeshData)value;
            }
            set
            {
                Api.ObjectCache["cosmosis:bigbeamtop"] = value;
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

        // public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        // {
        //     return (renderer != null);
        // }

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
