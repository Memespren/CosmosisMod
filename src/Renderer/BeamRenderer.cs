using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class BeamRenderer : IRenderer
    {

        // Modified Quern Renderer for BETransferPlanet

        private MeshRef baseref;

        private MeshRef topref;

        private MeshRef shotref;
        private Matrixf ModelMat = new Matrixf();
        private ICoreClientAPI api;
        private BlockPos pos;

        private Vec3f baseRot;

        private string facing;

        private float targetRot;

        private float targetPitch;

        private float rot;

        private float pitch;

        private float yRot;

        private float xRot;

        private float recoil = 0;

        private float shotlife = 0;

        private float shotduration = 0.15f;

        private float distance = 1;

        private float targetDistance = 1;

        public double RenderOrder
        {
            get { return 0.5; }
        }

        public int RenderRange
        {
            get { return 24; }
        }

        public BeamRenderer(ICoreClientAPI api, BlockPos pos, MeshData baseMesh, MeshData topMesh, MeshData shotMesh, string facing)
        {
            this.api = api;
            this.pos = pos;
            this.facing = facing;
            this.baseref = api.Render.UploadMesh(baseMesh);
            this.topref = api.Render.UploadMesh(topMesh);
            this.shotref = api.Render.UploadMesh(shotMesh);

            baseRot = new Vec3f();

            switch(facing)
            {
                case "west":
                    baseRot.Z = GameMath.PIHALF;
                    break;
                case "east":
                    baseRot.Z = -GameMath.PIHALF;
                    break;
                case "up":
                    baseRot.X = 0;
                    break;
                case "down":
                    baseRot.X = GameMath.PI;
                    break;
                case "north":
                    baseRot.X = -GameMath.PIHALF;
                    break;
                case "south":
                    baseRot.X = GameMath.PIHALF;
                    break;   
            }
        }

        public void OnRenderFrame(float dt, EnumRenderStage stage)
        {
            if (baseref == null || topref == null)
                return;

            IRenderAPI rpi = api.Render;
            Vec3d camPos = api.World.Player.Entity.CameraPos;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            prog.Tex2D = api.BlockTextureAtlas.AtlasTextureIds[0];

            prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 0.5f, 0.5f)
                .Rotate(baseRot)
                .RotateY(yRot)
                .Translate(-0.5f, -0.5f, -0.5f)
                .Values;

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(baseref);

            prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 0.5f, 0.5f)
                .Rotate(baseRot)
                .Translate(-0.5f, -0.5f, -0.5f)
                .Translate(0.5f, 0.625f, 0.5f)
                .RotateY(yRot)
                .RotateX(xRot)
                .Translate(-0.5f, -0.625f, -0.5f+recoil)
                .Values;

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(topref);

            if (shotlife > 0)
            {
                prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 0.5f, 0.5f)
                .Rotate(baseRot)
                .RotateY(yRot)
                .RotateX(xRot - GameMath.PIHALF)
                .RotateY(shotlife/shotduration * GameMath.PIHALF)
                .Scale(shotlife/shotduration, distance, shotlife/shotduration)
                .Translate(-0.5f, 0.75f/distance, -0.5f)
                .Values;

                prog.RgbaLightIn = new Vec4f(0, 1, 1, 0.8f);
                prog.RgbaGlowIn = new Vec4f(0, 1, 1, 0.8f);
                prog.ExtraGlow = 200;

                prog.ViewMatrix = rpi.CameraMatrixOriginf;
                prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
                rpi.RenderMesh(shotref);

                shotlife -= dt;
            }
            else
            {
                updateTurn(dt);
            }

            prog.Stop();
        }

        private void updateTurn(float dt)
        {
            distance = targetDistance;
            recoil = GameMath.Lerp(recoil, 0, 16*dt);
            rot = GameMath.Lerp(rot, targetRot, 32*dt);
            pitch = GameMath.Lerp(pitch, targetPitch, 32*dt);
            switch(facing)
            {
                case "east":
                    yRot = rot - GameMath.PIHALF;
                    xRot = pitch;
                    break;
                case "west":
                    yRot = rot;
                    xRot = -pitch;
                    break;
                case "down":
                    yRot = rot + GameMath.PIHALF;
                    xRot = -pitch;
                    break;
                case "up":
                    yRot = rot;
                    xRot = pitch;
                    break;
                case "south":
                    yRot = rot + GameMath.PIHALF;
                    xRot = pitch;
                    break;   
                case "north":
                    yRot = rot;
                    xRot = -pitch;
                    break;   
            }
        }

        public void Shoot()
        {
            shotlife = shotduration;
            recoil = 0.1f;
        }

        public void setTarget(BlockPos targetPos)
        {
            setTarget(targetPos.ToVec3d());
        }

        public void setTarget(Vec3d targetPos)
        {

            Vec3d blockpos = pos.ToVec3d().Add(0.01, 0.01, 0.01);

            targetDistance = blockpos.DistanceTo(targetPos) - 1;

            Vec3d fromPos = new Vec3d();
            Vec3d toPos = new Vec3d();
            switch(facing)
            {
                case "west":
                    fromPos.Set(blockpos.Y, blockpos.X, blockpos.Z);
                    toPos.Set(targetPos.Y, targetPos.X, targetPos.Z);
                    break;
                case "east":
                    fromPos.Set(blockpos.Z, blockpos.X, blockpos.Y);
                    toPos.Set(targetPos.Z, targetPos.X, targetPos.Y);
                    break;
                case "up":
                    fromPos.Set(blockpos.X, blockpos.Y, blockpos.Z);
                    toPos.Set(targetPos.X, targetPos.Y, targetPos.Z);
                    break;
                case "down":
                    fromPos.Set(blockpos.Z, blockpos.Y, blockpos.X);
                    toPos.Set(targetPos.Z, targetPos.Y, targetPos.X);
                    break;
                case "north":
                    fromPos.Set(blockpos.X, blockpos.Z, blockpos.Y);
                    toPos.Set(targetPos.X, targetPos.Z, targetPos.Y);
                    break;
                case "south":
                    fromPos.Set(blockpos.Y, blockpos.Z, blockpos.X);
                    toPos.Set(targetPos.Y, targetPos.Z, targetPos.X);
                    break;   
            }

            targetRot = (float)Math.Atan2(toPos.X - fromPos.X, toPos.Z - fromPos.Z) + GameMath.PI;
            if ((toPos.Z - fromPos.Z) >= 0)
                targetPitch = -(float)Math.Atan2((toPos.Y - fromPos.Y) * GameMath.Cos(targetRot), toPos.Z - fromPos.Z);
            else
                targetPitch = (float)Math.Atan2((toPos.Y - fromPos.Y) * GameMath.Cos(targetRot), -(toPos.Z - fromPos.Z));

            if (rot-GameMath.PI > targetRot)
                rot -= 2*GameMath.PI;
            else if (targetRot-GameMath.PI > rot)
                rot += 2*GameMath.PI;

        }

        public void Dispose()
        {
            api.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
            baseref.Dispose();
            topref.Dispose();
            shotref.Dispose();
        }
    }
}