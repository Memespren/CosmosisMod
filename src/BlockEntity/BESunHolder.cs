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
    public class BESunHolder : EnergySource
    {

        private SunRenderer renderer; // Rendering handler 

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
                        mesher.TesselateShape(block, Shape.TryGet(Api, "cosmosis:shapes/block/planet.json"), out mesh);
                        baseMesh = mesh;
                    }
                }
                //Register renderer
                renderer = new SunRenderer(api as ICoreClientAPI, Pos, baseMesh);
                (api as ICoreClientAPI).Event.RegisterRenderer(renderer, EnumRenderStage.Opaque, "sunholder");
            }
        }

        MeshData baseMesh
        {
            get
            {
                object value = null;
                Api.ObjectCache.TryGetValue("cosmosis:sunholder", out value);
                return (MeshData)value;
            }
            set
            {
                Api.ObjectCache["cosmosis:sunholder"] = value;
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
