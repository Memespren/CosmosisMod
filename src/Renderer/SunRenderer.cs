using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace cosmosis
{
    public class SunRenderer : IRenderer
    {

        // Modified Quern Renderer for BETransferPlanet

        private MeshRef meshref;
        private Matrixf ModelMat = new Matrixf();
        private ICoreClientAPI api;
        private BlockPos pos;

        private float angle;
        private float yoffset;

        public bool doRender;

        public double RenderOrder
        {
            get { return 0.5; }
        }

        public int RenderRange
        {
            get { return 24; }
        }

        public SunRenderer(ICoreClientAPI api, BlockPos pos, MeshData mesh)
        {
            this.api = api;
            this.pos = pos;
            this.meshref = api.Render.UploadMesh(mesh);
            this.doRender = true;
        }

        public void OnRenderFrame(float dt, EnumRenderStage stage)
        {
            if (meshref == null || !doRender)
                return;

            IRenderAPI rpi = api.Render;
            Vec3d camPos = api.World.Player.Entity.CameraPos;

            rpi.GlDisableCullFace();
            rpi.GlToggleBlend(true);

            IStandardShaderProgram prog = rpi.PreparedStandardShader(pos.X, pos.Y, pos.Z);
            prog.Tex2D = api.BlockTextureAtlas.AtlasTextures[0].TextureId;


            prog.ModelMatrix = ModelMat
                .Identity()
                .Translate(pos.X - camPos.X, pos.Y - camPos.Y, pos.Z - camPos.Z)
                .Translate(0.5f, 0.5f, 0.5f)
                .Scale(4, 4, 4)
                .RotateY(angle)
                .Translate(-0.5f, (yoffset+2f)/4, -0.5f)
                .Values;

            prog.RgbaLightIn = new Vec4f(1, 0.5f, 0, 0.8f);
            prog.RgbaGlowIn = new Vec4f(1, 0.5f, 0, 0.8f);
            prog.ExtraGlow = 255;


            prog.ViewMatrix = rpi.CameraMatrixOriginf;
            prog.ProjectionMatrix = rpi.CurrentProjectionMatrix;
            rpi.RenderMesh(meshref);
            prog.Stop();

            angle = (angle + (GameMath.PI / 4 * dt)) % (2 * GameMath.PI);
            yoffset = 0.5f * GameMath.Sin(angle);
        }

        public void Dispose()
        {
            api.Event.UnregisterRenderer(this, EnumRenderStage.Opaque);
            meshref.Dispose();
        }
    }
}