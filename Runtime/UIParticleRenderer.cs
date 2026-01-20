using UnityEngine;
using UnityEngine.UI;

namespace Waker.CanvasParticleSystems
{
    /// <summary>
    /// 기존 파티클 시스템을 활용한 UI용 파티클 렌더러 버전
    /// ParticleSystem의 파티클을 Canvas에 그리기 위해 MaskableGraphic을 상속받아 구현
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIParticleRenderer : MaskableGraphic
    {
        [SerializeField] private Texture particleTexture;

        private ParticleSystem _particleSystem;
        private ParticleSystem.Particle[] _particles;
        private UIVertex[] _quad = new UIVertex[4];
        private Vector2[] _uvs = new Vector2[4];

        private static readonly Vector2[] s_defaultUVs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0)
        };

        private static readonly Vector2[] s_quadOffsets = new Vector2[]
        {
            new Vector2(-1, -1),
            new Vector2(-1, 1),
            new Vector2(1, 1),
            new Vector2(1, -1)
        };

        private int _lastParticleCount;

        public override Texture mainTexture => particleTexture ? particleTexture : Texture2D.whiteTexture;

        protected override void Awake()
        {
            base.Awake();
            _particleSystem = GetComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = GetComponent<ParticleSystemRenderer>();
            if (renderer)
            {
                renderer.enabled = false;
            }
        }

        private void Update()
        {
            if (_particleSystem != null && (_particleSystem.isPlaying || _particleSystem.particleCount > 0))
            {
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (_particleSystem == null)
            {
                return;
            }

            int maxParticles = _particleSystem.main.maxParticles;
            if (_particles == null || _particles.Length < maxParticles)
            {
                _particles = new ParticleSystem.Particle[maxParticles];
            }

            int count = _particleSystem.GetParticles(_particles);

            if (count == 0)
            {
                return;
            }

            var sheetModule = _particleSystem.textureSheetAnimation;
            bool useSheet = sheetModule.enabled;
            int tilesX = useSheet ? sheetModule.numTilesX : 1;
            int tilesY = useSheet ? sheetModule.numTilesY : 1;
            int totalFrames = tilesX * tilesY;
            float uvStepX = 1.0f / tilesX;
            float uvStepY = 1.0f / tilesY;

            var mainModule = _particleSystem.main;
            var simulationSpace = mainModule.simulationSpace;

            bool isLocalSpace = simulationSpace == ParticleSystemSimulationSpace.Local;
            Matrix4x4 matrix = Matrix4x4.identity;

            if (!isLocalSpace)
            {
                if (simulationSpace == ParticleSystemSimulationSpace.World)
                {
                    matrix = transform.worldToLocalMatrix;
                }
                else if (simulationSpace == ParticleSystemSimulationSpace.Custom)
                {
                    var customSpace = mainModule.customSimulationSpace;
                    if (customSpace != null)
                    {
                        matrix = transform.worldToLocalMatrix * customSpace.localToWorldMatrix;
                    }
                    else
                    {
                        isLocalSpace = true;
                    }
                }
            }

            bool useDefaultUV = !useSheet || totalFrames <= 1;
            if (useDefaultUV)
            {
                _uvs[0] = s_defaultUVs[0];
                _uvs[1] = s_defaultUVs[1];
                _uvs[2] = s_defaultUVs[2];
                _uvs[3] = s_defaultUVs[3];
            }

            for (int i = 0; i < count; i++)
            {
                ref var p = ref _particles[i];

                Vector2 position = isLocalSpace
                    ? new Vector2(p.position.x, p.position.y)
                    : (Vector2)matrix.MultiplyPoint3x4(p.position);

                float size = p.GetCurrentSize(_particleSystem) * 0.5f;
                Color32 color = p.GetCurrentColor(_particleSystem);

                float angle = -p.rotation * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                if (!useDefaultUV)
                {
                    float lifeRatio = 1f - (p.remainingLifetime / p.startLifetime);
                    int frameIndex = Mathf.Clamp((int)(lifeRatio * totalFrames), 0, totalFrames - 1);

                    int xIndex = frameIndex % tilesX;
                    int yIndex = tilesY - 1 - (frameIndex / tilesX);

                    float uMin = xIndex * uvStepX;
                    float vMin = yIndex * uvStepY;
                    float uMax = uMin + uvStepX;
                    float vMax = vMin + uvStepY;

                    _uvs[0] = new Vector2(uMin, vMin);
                    _uvs[1] = new Vector2(uMin, vMax);
                    _uvs[2] = new Vector2(uMax, vMax);
                    _uvs[3] = new Vector2(uMax, vMin);
                }

                for (int j = 0; j < 4; j++)
                {
                    float offsetX = s_quadOffsets[j].x * size;
                    float offsetY = s_quadOffsets[j].y * size;

                    float x = (offsetX * cos) - (offsetY * sin);
                    float y = (offsetX * sin) + (offsetY * cos);

                    _quad[j].position = new Vector3(position.x + x, position.y + y, 0);
                    _quad[j].color = color;
                    _quad[j].uv0 = _uvs[j];
                }

                vh.AddUIVertexQuad(_quad);
            }
        }
    }
}