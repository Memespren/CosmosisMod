using System;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class BigBeamRenderer : IRenderer
    {

        // Modified Quern Renderer for BETransferPlanet

        private MeshRef baseref;

        private MeshRef topref;

        private MeshRef shotref;
        private Matrixf ModelMat = new Matrixf();
        private ICoreClientAPI api;
        private BlockPos pos;

        private float targetRot;

        private float targetPitch;

        private float rot;

        private float pitch;

        private float recoil = 0;

        private float shotlife = 0;

        private float shotduration = 0.2f;

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

        public BigBeamRenderer(ICoreClientAPI api, BlockPos pos, MeshData baseMesh, MeshData topMesh, MeshData shotMesh)
        {
            this.api = api;
            this.pos = pos;
            this.baseref = api.Render.UploadMesh(baseMesh);
            this.topref = api.Render.UploadMesh(topMesh);
            this.shotref = api.Render.UploadMesh(shotMesh);
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
                .RotateY(rot)
                .Translate(-0.5f, -0.5f, -0.5f)
                .Values;

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(baseref);


            prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 0.5f, 0.5f)
                .RotateY(rot)
                .Translate(-0.5f, -0.5f, -0.5f)
                .Translate(0.5f, 2.25f, 1.125f)
                .RotateX(pitch)
                .Translate(-0.5f, -2.25f, -1.125f+recoil)
                .Values;

            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(topref);

            if (shotlife > 0)
            {
                prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 2.25f + (pitch/GameMath.PIHALF), 0.5f)
                .RotateY(rot)
                .RotateX(pitch - GameMath.PIHALF)
                .RotateY(shotlife/shotduration * GameMath.PIHALF)
                .Scale(4*(shotlife/shotduration), distance, 4*(shotlife/shotduration))
                .Translate(-0.5f, 1.75f/distance, -0.5f)
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
            recoil = GameMath.Lerp(recoil, 0, dt);
            rot = GameMath.Lerp(rot, targetRot, 3*dt);
            pitch = GameMath.Lerp(pitch, targetPitch, 3*dt);
        }

        public void Shoot()
        {
            shotlife = shotduration;
            recoil = 0.4f;
        }

        public void setTarget(BlockPos targetPos)
        {
            setTarget(targetPos.ToVec3d());
        }

        public void setTarget(Vec3d targetPos)
        {

            Vec3d blockpos = pos.ToVec3d().Add(0.01, 2.01, 0.01);

            targetDistance = blockpos.DistanceTo(targetPos) - 1;

            targetRot = (float)Math.Atan2(targetPos.X - blockpos.X, targetPos.Z - blockpos.Z) + GameMath.PI;
            if ((targetPos.Z - blockpos.Z) >= 0)
                targetPitch = -(float)Math.Atan2((targetPos.Y - blockpos.Y) * GameMath.Cos(targetRot), targetPos.Z - blockpos.Z);
            else
                targetPitch = (float)Math.Atan2((targetPos.Y - blockpos.Y) * GameMath.Cos(targetRot), -(targetPos.Z - blockpos.Z));

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