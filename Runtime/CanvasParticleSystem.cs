using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace Waker.CanvasParticleSystems
{
    /// <summary>
    /// 버스트 설정
    /// </summary>
    [Serializable]
    public class BurstEntry
    {
        [Tooltip("버스트 발생 시간")]
        public float time = 0f;
        
        [Tooltip("버스트 파티클 개수")]
        public ParticleSystem.MinMaxCurve count = new ParticleSystem.MinMaxCurve(10);
        
        [Tooltip("버스트 반복 횟수 (0 = 무한)")]
        public int cycles = 1;
        
        [Tooltip("버스트 반복 간격")]
        public float interval = 0.01f;
        
        // 내부 상태
        [NonSerialized] public int cycleIndex;
        [NonSerialized] public float lastBurstTime;
    }

    /// <summary>
    /// 방출 설정 그룹
    /// </summary>
    [Serializable]
    public class EmissionModule
    {
        [Tooltip("자동 방출 활성화")]
        public bool enabled = false;
        
        [Tooltip("초당 방출 개수")]
        public int rateOverTime = 10;
        
        [Tooltip("버스트 목록")]
        public List<BurstEntry> bursts = new List<BurstEntry>();
    }

    /// <summary>
    /// 파티클 초기 설정 그룹
    /// </summary>
    [Serializable]
    public class MainModule
    {
        [Tooltip("시스템 지속 시간")]
        public float duration = 5f;
        
        [Tooltip("루프 여부")]
        public bool loop = true;
        
        [Tooltip("Awake 시 자동 재생")]
        public bool playOnAwake = true;
        
        [Tooltip("파티클 수명")]
        public ParticleSystem.MinMaxCurve startLifetime = new ParticleSystem.MinMaxCurve(1f, 3f);
        
        [Tooltip("파티클 시작 크기")]
        public ParticleSystem.MinMaxCurve startSize = new ParticleSystem.MinMaxCurve(10f, 20f);
        
        [Tooltip("파티클 시작 속도")]
        public ParticleSystem.MinMaxCurve startSpeed = new ParticleSystem.MinMaxCurve(50f, 150f);
        
        [Tooltip("파티클 시작 회전 (도)")]
        public ParticleSystem.MinMaxCurve startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
        
        [Tooltip("파티클 회전 속도 (도/초)")]
        public ParticleSystem.MinMaxCurve angularVelocity = new ParticleSystem.MinMaxCurve(-180f, 180f);
        
        [Tooltip("파티클 시작 색상")]
        public ParticleSystem.MinMaxGradient startColor = new ParticleSystem.MinMaxGradient(Color.white);
    }

    /// <summary>
    /// 속도 변화 모듈
    /// </summary>
    [Serializable]
    public class VelocityOverLifetimeModule
    {
        [Tooltip("속도 변화 활성화")]
        public bool enabled = false;
        
        [Tooltip("X 방향 속도")]
        public ParticleSystem.MinMaxCurve velocityX = new ParticleSystem.MinMaxCurve(0f);
        
        [Tooltip("Y 방향 속도")]
        public ParticleSystem.MinMaxCurve velocityY = new ParticleSystem.MinMaxCurve(0f);
    }

    /// <summary>
    /// 노이즈 모듈 - Perlin Noise 기반 움직임
    /// </summary>
    [Serializable]
    public class NoiseModule
    {
        [Tooltip("노이즈 활성화")]
        public bool enabled = false;
        
        [Tooltip("노이즈 강도")]
        public ParticleSystem.MinMaxCurve strength = new ParticleSystem.MinMaxCurve(0f, 10f);
        
        [Tooltip("노이즈 주파수 - 값이 클수록 빠르게 변화")]
        [Range(0.1f, 10f)]
        public float frequency = 1f;
        
        [Tooltip("노이즈 스크롤 속도 - 시간에 따른 노이즈 패턴 이동")]
        public Vector2 scrollSpeed = Vector2.zero;
    }

    /// <summary>
    /// 크기 변화 그룹
    /// </summary>
    [Serializable]
    public class SizeOverLifetimeModule
    {
        [Tooltip("크기 변화 활성화")]
        public bool enabled = false;
        
        [Tooltip("수명에 따른 크기 배율")]
        public ParticleSystem.MinMaxCurve size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        ));
    }

    /// <summary>
    /// 색상 변화 그룹
    /// </summary>
    [Serializable]
    public class ColorOverLifetimeModule
    {
        [Tooltip("색상 변화 활성화")]
        public bool enabled = false;
        
        [Tooltip("수명에 따른 색상")]
        public ParticleSystem.MinMaxGradient color = new ParticleSystem.MinMaxGradient(
            new Gradient
            {
                colorKeys = new GradientColorKey[] 
                { 
                    new GradientColorKey(Color.white, 0f), 
                    new GradientColorKey(Color.white, 1f) 
                },
                alphaKeys = new GradientAlphaKey[] 
                { 
                    new GradientAlphaKey(1f, 0f), 
                    new GradientAlphaKey(0f, 1f) 
                }
            }
        );
    }

    /// <summary>
    /// 흡입/끌어당김 모듈
    /// </summary>
    [Serializable]
    public class AttractionModule
    {
        [Tooltip("Attraction 활성화")]
        public bool enabled = false;
        
        [Tooltip("끌어당길 대상 Transform")]
        public Transform? target;
        
        [Tooltip("수명에 따른 끌어당김 정도 (0~1: 0=물리위치, 1=타겟위치)")]
        public ParticleSystem.MinMaxCurve amount = new ParticleSystem.MinMaxCurve(0f, new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(1f, 1f)
        ));
    }

    /// <summary>
    /// 텍스처 시트 애니메이션 모드
    /// </summary>
    public enum TextureSheetAnimationMode
    {
        /// <summary>
        /// 초당 프레임 수 기반
        /// </summary>
        FPS,

        /// <summary>
        /// 파티클 수명 동안 애니메이션 완료
        /// </summary>
        Lifetime
    }

    /// <summary>
    /// 렌더러 모듈
    /// </summary>
    [Serializable]
    public class RendererModule
    {
        [Tooltip("파티클 스프라이트")]
        public Sprite? mainSprite;
        
        [Tooltip("렌더러 풀 최대 크기 (동시에 사용 가능한 다른 스프라이트 개수)")]
        [Range(4, 64)]
        public int maxRendererPoolSize = 16;
    }

    /// <summary>
    /// 텍스처 시트 애니메이션 모듈
    /// </summary>
    [Serializable]
    public class TextureSheetAnimationModule
    {
        [Tooltip("시트 애니메이션 활성화")]
        public bool enabled = false;

        [Tooltip("애니메이션 모드\n- FPS: 초당 프레임 수 기반\n- Lifetime: 수명 동안 사이클 완료")]
        public TextureSheetAnimationMode mode = TextureSheetAnimationMode.FPS;

        [Tooltip("타일 X")]
        [Range(1, 16)]
        public int tilesX = 1;

        [Tooltip("타일 Y")]
        [Range(1, 16)]
        public int tilesY = 1; 

        [Tooltip("초당 프레임 수 (FPS 모드)")]
        [Range(0.1f, 120)]
        public float fps = 10f;
        
        [Tooltip("수명 동안 반복 횟수 (Lifetime 모드)")]
        [Range(0.1f, 10)]
        public float cycles = 1f;
        
        [Tooltip("시작 프레임 인덱스 (0부터 시작)")]
        public int startFrame = 0;
        
        [Tooltip("끝 프레임 인덱스 (-1: 마지막 프레임)")]
        public int endFrame = -1;
    }

    /// <summary>
    /// 방출 모양 설정
    /// </summary>
    public enum EmissionShape
    {
        Circle,
        Cone,
        Edge,
        Rectangle
    }

    /// <summary>
    /// 스프레드 모드
    /// </summary>
    public enum SpreadMode
    {
        /// <summary>
        /// 랜덤 분산
        /// </summary>
        Random,
        
        /// <summary>
        /// 순차적 분산 (루프)
        /// </summary>
        Loop,
        
        /// <summary>
        /// 왕복 패턴
        /// </summary>
        PingPong,
        
        /// <summary>
        /// 버스트 시 균등 분배
        /// </summary>
        BurstSpread
    }

    /// <summary>
    /// Shape 모듈
    /// </summary>
    [Serializable]
    public class ShapeModule
    {
        [Tooltip("Shape 활성화")]
        public bool enabled = true;

        [Tooltip("기준 Transform (null이면 파티클 시스템 위치 사용)")]
        public Transform? pivot;

        [Tooltip("방출 모양")]
        public EmissionShape shape = EmissionShape.Circle;

        [Tooltip("방출 각도 (도) - 모든 Shape에 적용")]
        public float emissionAngle = 90f;

        [Tooltip("반지름 (Circle, Cone)")]
        public float radius = 50f;

        [Tooltip("각도 범위 (Cone)")]
        public float angle = 45f;

        [Tooltip("크기 (Rectangle, Edge)")]
        public Vector2 size = new Vector2(100f, 100f);

        [Tooltip("스프레드 모드\n- Circle/Cone/Rectangle: 중앙으로부터 각도 스프레드\n- Edge: 라인 위치 스프레드")]
        public SpreadMode spreadMode = SpreadMode.Random;

        [Tooltip("스프레드 속도 (Loop/PingPong 모드)\n값이 클수록 빠르게 순환")]
        [Range(0.01f, 10f)]
        public float spreadSpeed = 1f;

        [Tooltip("위치 랜덤니스 (0=중앙, 1=랜덤 거리)\nCircle/Cone/Rectangle에 적용")]
        [Range(0f, 1f)]
        public float positionRandomness = 1f;

        [Tooltip("방향 랜덤니스 (0=위치 각도, 1=랜덤 방향)")]
        [Range(0f, 1f)]
        public float directionRandomness = 0f;

        // 내부 상태
        [NonSerialized] public float spreadLoopProgress = 0f;
        [NonSerialized] public bool spreadPingPongReverse = false;
    }

    /// <summary>
    /// Canvas 기반 파티클 시스템
    /// Graphic을 상속하여 하나의 Mesh에 모든 파티클을 배치 렌더링합니다.
    /// 이 컴포넌트 하나로 파티클 시스템의 모든 기능을 제어합니다.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasRenderer))]
    public class CanvasParticleSystem : MaskableGraphic
    {
        [SerializeField] private MainModule _mainModule = new MainModule();
        [SerializeField] private EmissionModule _emissionModule = new EmissionModule();
        [SerializeField] private ShapeModule _shapeModule = new ShapeModule();
        [SerializeField] private VelocityOverLifetimeModule _velocityOverLifetime = new VelocityOverLifetimeModule();
        [SerializeField] private NoiseModule _noiseModule = new NoiseModule();
        [SerializeField] private SizeOverLifetimeModule _sizeOverLifetime = new SizeOverLifetimeModule();
        [SerializeField] private ColorOverLifetimeModule _colorOverLifetime = new ColorOverLifetimeModule();
        [SerializeField] private AttractionModule _attractionModule = new AttractionModule();
        [SerializeField] private RendererModule _rendererModule = new RendererModule();
        [SerializeField] private TextureSheetAnimationModule _textureSheetAnimation = new TextureSheetAnimationModule();
        [SerializeField] private SubEmitterModule _subEmitterModule = new SubEmitterModule();

        private ParticleData[] _particles = new ParticleData[0];
        private int _activeParticleCount;
        private int _maxParticles = 1000;
        private bool _isInitialized;

        private int _lastFreeIndex = 0;
        
        // 파티클 ID 생성용 카운터 (인덱스 재사용 시 유효성 검증)
        private uint _particleIdCounter = 0;

        // Emission 관리 (Sub Emitter 전용)
        private List<ParticleEmission> _emissions = new List<ParticleEmission>();
        private int _nextEmissionId = 0;
        
        // 자체 Emission 상태 (별도 ParticleEmission 객체 없이 직접 관리)
        private float _playTime; // 재생 시간
        private float _emissionAccumulator; // Rate Over Time 누적
        private List<BurstState> _burstStates = new List<BurstState>();
        
        // Sub Emitter Depth 제한 (1 depth만 허용)
        // 0: 일반 시스템, 1: SubEmitter로 작동 중인 시스템
        [NonSerialized] internal int _subEmitterDepth = 0;

        // 시뮬레이션 상태 필드
        private bool _isPlaying; // 재생 중 여부
        private bool _isPaused; // 일시정지 여부
        private bool _isManuallyPaused; // 사용자가 수동으로 일시정지한 여부
        private float _simulationTime; // 전체 시뮬레이션 시간
        private float _lastUpdateTime; // 마지막 업데이트 시간
        
        // Sprite Override를 위한 렌더러 풀
        private ParticleRendererPool? _rendererPool;

        // 모듈 접근자
        public MainModule Main => _mainModule;
        public EmissionModule Emission => _emissionModule;
        public ShapeModule Shape => _shapeModule;
        public VelocityOverLifetimeModule VelocityOverLifetime => _velocityOverLifetime;
        public NoiseModule Noise => _noiseModule;
        public SizeOverLifetimeModule SizeOverLifetime => _sizeOverLifetime;
        public ColorOverLifetimeModule ColorOverLifetime => _colorOverLifetime;
        public AttractionModule Attraction => _attractionModule;
        public RendererModule Renderer => _rendererModule;
        public TextureSheetAnimationModule TextureSheetAnimation => _textureSheetAnimation;
        public SubEmitterModule SubEmitter => _subEmitterModule;

        public override Texture mainTexture
        {
            get
            {
                if (_rendererModule.mainSprite != null)
                {
                    var tex = _rendererModule.mainSprite.texture;
                    if (tex != null)
                        return tex;
                }
                return base.mainTexture;
            }
        }

        /// <summary>
        /// 최대 파티클 개수
        /// </summary>
        public int MaxParticles
        {
            get => _maxParticles;
            set
            {
                if (_maxParticles != value)
                {
                    _maxParticles = Mathf.Max(1, value);
                    if (_isInitialized)
                    {
                        Cleanup();
                        Initialize();
                    }
                }
            }
        }

        /// <summary>
        /// 현재 활성화된 파티클 개수
        /// </summary>
        public int ActiveParticleCount => _activeParticleCount;

        /// <summary>
        /// 시뮬레이션이 재생 중인지 여부
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// 시뮬레이션이 일시 정지 중인지 여부
        /// </summary>
        public bool IsPaused => _isPaused;
        
        #region Particle Access Helpers
        
        /// <summary>
        /// 파티클이 살아있는지 확인
        /// </summary>
        internal bool IsParticleAlive(int index)
        {
            if (index < 0 || index >= _particles.Length) return false;
            return _particles[index].IsAlive;
        }
        
        /// <summary>
        /// 파티클이 살아있고 ID가 일치하는지 확인
        /// </summary>
        internal bool IsParticleValid(int index, uint particleId)
        {
            if (index < 0 || index >= _particles.Length) return false;
            var particle = _particles[index];
            return particle.IsAlive && particle.Id == particleId;
        }
        
        /// <summary>
        /// 파티클 위치 반환
        /// </summary>
        internal Vector2 GetParticlePosition(int index)
        {
            if (index < 0 || index >= _particles.Length) return Vector2.zero;
            var pos = _particles[index].Position;
            return new Vector2(pos.x, pos.y);
        }
        
        /// <summary>
        /// 파티클 속도 반환
        /// </summary>
        internal Vector2 GetParticleVelocity(int index)
        {
            if (index < 0 || index >= _particles.Length) return Vector2.zero;
            var vel = _particles[index].Velocity;
            return new Vector2(vel.x, vel.y);
        }
        
        /// <summary>
        /// 파티클 색상 반환
        /// </summary>
        internal Color GetParticleColor(int index)
        {
            if (index < 0 || index >= _particles.Length) return Color.white;
            var col = _particles[index].Color;
            return new Color(col.x, col.y, col.z, col.w);
        }
        
        /// <summary>
        /// 파티클 크기 반환
        /// </summary>
        internal float GetParticleSize(int index)
        {
            if (index < 0 || index >= _particles.Length) return 1f;
            return _particles[index].Size;
        }
        
        /// <summary>
        /// 파티클 회전 반환 (라디안)
        /// </summary>
        internal float GetParticleRotation(int index)
        {
            if (index < 0 || index >= _particles.Length) return 0f;
            return _particles[index].Rotation;
        }
        
        /// <summary>
        /// 파티클 ID 반환
        /// </summary>
        internal uint GetParticleId(int index)
        {
            if (index < 0 || index >= _particles.Length) return 0;
            return _particles[index].Id;
        }
        
        /// <summary>
        /// 파티클 데이터 전체 반환
        /// </summary>
        internal ParticleData GetParticleData(int index)
        {
            if (index < 0 || index >= _particles.Length) return default;
            return _particles[index];
        }
        
        #endregion

        /// <summary>
        /// 현재 시뮬레이션 시간 (초)
        /// </summary>
        public float SimulationTime => _playTime;

        protected override void Awake()
        {
            base.Awake();
            Initialize();
            
            // Play On Awake 설정 확인
            if (_mainModule.playOnAwake)
            {
                Play(playChildren: false);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            // 에디터 모드에서도 초기화 필요
            if (!_isInitialized)
                Initialize();
                
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.update += EditorUpdate;
            }
#endif
        }

        protected override void OnDisable()
        {
            base.OnDisable();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.update -= EditorUpdate;
            }
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cleanup();
        }

        private void Initialize()
        {
            if (_isInitialized)
                return;

            _particles = new ParticleData[_maxParticles];
            _activeParticleCount = 0;
            
            _isInitialized = true;
        }

        private void Cleanup()
        {
            // 렌더러 풀 정리
            _rendererPool?.Destroy();
            _rendererPool = null;
            
            _particles = null!;
            _isInitialized = false;
        }

        /// <summary>
        /// deltaTime 계산 (에디터/런타임 통합)
        /// </summary>
        private float GetDeltaTime()
        {
            float currentTime = Time.realtimeSinceStartup;
            float deltaTime = currentTime - _lastUpdateTime;
            _lastUpdateTime = currentTime;

            // deltaTime이 너무 크면 제한 (탭 전환, 에디터 일시정지 등)
            return Mathf.Min(deltaTime, 0.1f);
        }

        /// <summary>
        /// 통합 업데이트 처리
        /// </summary>
        private void UpdateInternal()
        {
            if (!_isInitialized)
                return;

            // Sub Emitter로 작동 중인 시스템은 항상 업데이트 (부모가 트리거)
            // 일반 시스템은 Playing 상태일 때만 업데이트
            if (_subEmitterDepth == 0 && !_isPlaying && !_isPaused)
                return;

            float deltaTime = GetDeltaTime();
            SimulateInternal(deltaTime);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터 모드에서의 업데이트 (EditorApplication.update에서 호출)
        /// </summary>
        private void EditorUpdate()
        {
            if (Application.isPlaying)
                return;

            // Sub Emitter로 작동 중인 시스템은 부모가 선택되어 있으면 업데이트
            // 일반 시스템은 자신이 선택되어 있어야 업데이트
            bool shouldUpdate = false;
            
            if (_subEmitterDepth > 0)
            {
                // Sub Emitter 자식은 항상 업데이트 (부모가 관리)
                shouldUpdate = true;
            }
            else if (_isPlaying || _isPaused)
            {
                // 일반 시스템은 Playing/Paused 상태일 때만
                shouldUpdate = true;
                
                // 선택 상태 확인 - 선택되지 않았으면 일시정지 (수동 일시정지는 예외)
                bool isSelectedOrChild = IsSelectedInHierarchy();
                if (!isSelectedOrChild && _isPlaying && !_isPaused && !_isManuallyPaused)
                {
                    _isPaused = true;
                    // 서브 시스템도 일시정지
                    PauseSubEmitterChildren();
                }
                else if (isSelectedOrChild && _isPaused && !_isManuallyPaused)
                {
                    _isPaused = false;
                    _lastUpdateTime = Time.realtimeSinceStartup;
                    // 서브 시스템도 재개
                    ResumeSubEmitterChildren();
                }
            }
            
            if (!shouldUpdate)
                return;

            UpdateInternal();

            // 시뮬레이션 중이면 씬 뷰와 게임 뷰를 강제로 다시 그리게 함
            if (_isPlaying && !_isPaused)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
            }
        }

        /// <summary>
        /// 현재 오브젝트 또는 하위 오브젝트가 선택되어 있는지 확인
        /// </summary>
        private bool IsSelectedInHierarchy()
        {
            if (UnityEditor.Selection.activeGameObject == null)
                return false;

            // 자신이 선택되었는지
            if (UnityEditor.Selection.activeGameObject == gameObject)
                return true;

            // 자신의 부모 중에 선택된 객체가 있는지 (하위 객체인 경우)
            Transform current = transform;
            while (current != null)
            {
                if (UnityEditor.Selection.activeGameObject == current.gameObject)
                    return true;
                current = current.parent;
            }

            // 자신의 자식 중에 선택된 객체가 있는지
            return UnityEditor.Selection.activeGameObject.transform.IsChildOf(transform);
        }
#endif

        private void Update()
        {
#if UNITY_EDITOR
            // 에디터 모드에서는 EditorUpdate에서 처리
            if (!Application.isPlaying)
                return;
#endif
            UpdateInternal();
        }

        /// <summary>
        /// 내부 시뮬레이션 처리 - Emission 기반
        /// </summary>
        private void SimulateInternal(float deltaTime)
        {
            _simulationTime += deltaTime;

            // Pause 상태면 Emission 업데이트와 파티클 생성 스킵
            if (_isPaused)
            {
                // 활성 파티클 수만 계산
                _activeParticleCount = 0;
                for (int i = 0; i < _particles.Length; i++)
                {
                    if (_particles[i].IsAlive) _activeParticleCount++;
                }
                SetVerticesDirty();
                return;
            }
            
            // Sub Emitter 자식이지만 Sub Emission이 없으면 파티클만 업데이트
            bool hasSubEmissions = _emissions.Exists(e => e.IsSubEmitter);
            if (_subEmitterDepth > 0 && !hasSubEmissions && _activeParticleCount == 0)
            {
                // 활성 파티클도 없고 Sub Emission도 없으면 depth 리셋
                _subEmitterDepth = 0;
                return;
            }
            
            // 자체 Emission 처리 (Sub Emitter로 작동 중이 아닐 때만)
            if (_subEmitterDepth == 0 && _isPlaying)
            {
                _playTime += deltaTime;
                
                // Duration 및 Loop 처리
                if (!_mainModule.loop && _playTime >= _mainModule.duration)
                {
                    // Duration 만료 시 방출 중지 (파티클은 계속 업데이트)
                    // _isPlaying은 유지 (파티클이 사라지면 자동 종료)
                }
                else
                {
                    // Loop 시 PlayTime 리셋
                    if (_mainModule.loop && _playTime >= _mainModule.duration)
                    {
                        _playTime = 0f;
                        // Burst 상태 리셋
                        foreach (var burst in _burstStates)
                        {
                            burst.CurrentCycle = 0;
                            burst.LastBurstTime = -1f;
                        }
                    }
                    
                    // Rate Over Time 방출
                    if (_emissionModule.enabled)
                    {
                        _emissionAccumulator += _emissionModule.rateOverTime * deltaTime;
                        int emitCount = Mathf.FloorToInt(_emissionAccumulator);
                        _emissionAccumulator -= emitCount;
                        
                        for (int i = 0; i < emitCount; i++)
                        {
                            EmitSingleInternal();
                        }
                    }
                    
                    // Burst 방출
                    ProcessSelfBursts();
                }
            }

            // Sub Emission 업데이트
            for (int i = _emissions.Count - 1; i >= 0; i--)
            {
                var emission = _emissions[i];
                
                // Sub Emitter: 부모 파티클 생존 체크 및 위치 업데이트
                if (emission.TrackParentPosition)
                {
                    // 부모 시스템과 파티클 유효성 체크
                    if (emission.ParentSystem == null || 
                        !emission.ParentSystem.IsParticleValid(emission.ParentParticleIndex, emission.ParentParticleId))
                    {
                        // 부모 파티클 소멸 시 세션 제거
                        _emissions.RemoveAt(i);
                        continue;
                    }
                    
                    // 부모 위치 지속 추적 (Position 상속 플래그 활성 시)
                    if ((emission.InheritFlags & SubEmitterInherit.Position) != 0)
                    {
                        emission.BasePosition = emission.ParentSystem.GetParticlePosition(emission.ParentParticleIndex);
                    }
                }
                
                emission.PlayTime += deltaTime;
                
                // Duration 만료 체크
                if (!emission.IsAlive)
                {
                    _emissions.RemoveAt(i);
                    
                    // 모든 Sub Emission이 제거되면 depth 초기화
                    if (_emissions.Count == 0)
                    {
                        _subEmitterDepth = 0;
                    }
                    continue;
                }
                
                // Rate Over Time 방출
                if (_emissionModule.enabled)
                {
                    ProcessRateOverTime(emission, deltaTime);
                }
                
                // Burst 방출
                ProcessBursts(emission, deltaTime);
            }
            
            // 파티클 업데이트
            UpdateParticles(deltaTime);

            // 활성 파티클 수 계산
            _activeParticleCount = 0;
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i].IsAlive) _activeParticleCount++;
            }
            
            // 빈 렌더러 정리 및 Dirty 갱신
            if (_rendererPool != null)
            {
                _rendererPool.CleanupEmptyRenderers();
                _rendererPool.MarkAllDirty();
            }
            
            SetVerticesDirty();
        }

        /// <summary>
        /// Rate Over Time 방출 처리 (Sub Emission용)
        /// </summary>
        private void ProcessRateOverTime(ParticleEmission emission, float deltaTime)
        {
            emission.EmissionAccumulator += _emissionModule.rateOverTime * deltaTime;
            
            int emitCount = Mathf.FloorToInt(emission.EmissionAccumulator);
            emission.EmissionAccumulator -= emitCount;
            
            for (int i = 0; i < emitCount; i++)
            {
                EmitFromSubEmission(emission);
            }
        }
        
        /// <summary>
        /// 자체 Burst 방출 처리
        /// </summary>
        private void ProcessSelfBursts()
        {
            foreach (var burstState in _burstStates)
            {
                var burst = burstState.BurstEntry;
                
                // 시간 체크
                float normalizedTime = _mainModule.duration > 0 ? (_playTime / _mainModule.duration) : 0f;
                if (_playTime >= burst.time && burstState.LastBurstTime < 0)
                {
                    // 첫 Burst
                    int count = Mathf.RoundToInt(burst.count.Evaluate(normalizedTime, UnityEngine.Random.value));
                    for (int i = 0; i < count; i++)
                    {
                        EmitSingleInternal();
                    }
                    
                    burstState.LastBurstTime = _playTime;
                    burstState.CurrentCycle = 1;
                }
                else if ((burst.cycles == 0 || burstState.CurrentCycle < burst.cycles) && burstState.LastBurstTime >= 0)
                {
                    // 반복 Burst (cycles == 0은 무한 반복)
                    if (_playTime >= burstState.LastBurstTime + burst.interval)
                    {
                        int count = Mathf.RoundToInt(burst.count.Evaluate(normalizedTime, UnityEngine.Random.value));
                        for (int i = 0; i < count; i++)
                        {
                            EmitSingleInternal();
                        }
                        
                        burstState.LastBurstTime = _playTime;
                        if (burst.cycles > 0) burstState.CurrentCycle++;
                    }
                }
            }
        }

        /// <summary>
        /// Burst 방출 처리 (Sub Emission용)
        /// </summary>
        private void ProcessBursts(ParticleEmission emission, float deltaTime)
        {
            foreach (var burstState in emission.BurstStates)
            {
                var burst = burstState.BurstEntry;
                
                // 시간 체크
                float normalizedTime = emission.Duration > 0 ? (emission.PlayTime / emission.Duration) : 0f;
                if (emission.PlayTime >= burst.time && burstState.LastBurstTime < 0)
                {
                    // 첫 Burst
                    int count = Mathf.RoundToInt(burst.count.Evaluate(normalizedTime, UnityEngine.Random.value));
                    for (int i = 0; i < count; i++)
                    {
                        EmitFromSubEmission(emission);
                    }
                    
                    burstState.LastBurstTime = emission.PlayTime;
                    burstState.CurrentCycle = 1;
                }
                else if (burst.cycles > 1 && burstState.CurrentCycle < burst.cycles)
                {
                    // 반복 Burst
                    if (emission.PlayTime >= burstState.LastBurstTime + burst.interval)
                    {
                        int count = Mathf.RoundToInt(burst.count.Evaluate(normalizedTime, UnityEngine.Random.value));
                        for (int i = 0; i < count; i++)
                        {
                            EmitFromSubEmission(emission);
                        }
                        
                        burstState.LastBurstTime = emission.PlayTime;
                        burstState.CurrentCycle++;
                    }
                }
            }
        }

        /// <summary>
        /// Sub Emitter 세션에서 파티클 방출 - 자식 시스템의 모든 모듈 적용
        /// </summary>
        private void EmitFromSubEmission(ParticleEmission emission)
        {
            // 1. Shape 모듈로 로컬 위치/방향 계산
            Vector2 localPosition = Vector2.zero;
            Vector2 localDirection = Vector2.up;

            if (_shapeModule.enabled)
            {
                float baseAngle = _shapeModule.emissionAngle * Mathf.Deg2Rad;
                float spread = GetSpread();

                switch (_shapeModule.shape)
                {
                    case EmissionShape.Circle:
                        {
                            float angle = spread * 2f * Mathf.PI;
                            float radius = Mathf.Lerp(0f, UnityEngine.Random.Range(0f, _shapeModule.radius), _shapeModule.positionRandomness);
                            localPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                            localPosition = RotateVector(localPosition, baseAngle);
                            float dirAngle = Mathf.Lerp(angle, UnityEngine.Random.Range(0f, 2f * Mathf.PI), _shapeModule.directionRandomness);
                            localDirection = RotateVector(new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle)), baseAngle);
                        }
                        break;

                    case EmissionShape.Cone:
                        {
                            float angleOffset = (spread - 0.5f) * _shapeModule.angle * Mathf.Deg2Rad;
                            float radius = Mathf.Lerp(0f, UnityEngine.Random.Range(0f, _shapeModule.radius), _shapeModule.positionRandomness);
                            float totalAngle = baseAngle + angleOffset;
                            localPosition = new Vector2(Mathf.Cos(totalAngle), Mathf.Sin(totalAngle)) * radius;
                            float halfAngle = _shapeModule.angle * 0.5f * Mathf.Deg2Rad;
                            float randomAngleOffset = UnityEngine.Random.Range(-halfAngle, halfAngle);
                            float dirAngle = Mathf.Lerp(totalAngle, baseAngle + randomAngleOffset, _shapeModule.directionRandomness);
                            localDirection = new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle));
                        }
                        break;

                    case EmissionShape.Edge:
                        {
                            float edgePos = (spread - 0.5f) * _shapeModule.size.x;
                            Vector2 edgePerp = new Vector2(-Mathf.Sin(baseAngle), Mathf.Cos(baseAngle));
                            localPosition = edgePerp * edgePos;
                            float dirAngle = Mathf.Lerp(baseAngle, UnityEngine.Random.Range(0f, 2f * Mathf.PI), _shapeModule.directionRandomness);
                            localDirection = new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle));
                        }
                        break;

                    case EmissionShape.Rectangle:
                        {
                            float angle = spread * 2f * Mathf.PI;
                            float halfWidth = _shapeModule.size.x * 0.5f;
                            float halfHeight = _shapeModule.size.y * 0.5f;
                            float maxDistance = Mathf.Min(halfWidth, halfHeight);
                            float distance = Mathf.Lerp(0f, UnityEngine.Random.Range(0f, maxDistance), _shapeModule.positionRandomness);
                            localPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                            localPosition = RotateVector(localPosition, baseAngle);
                            float dirAngle = Mathf.Lerp(angle, UnityEngine.Random.Range(0f, 2f * Mathf.PI), _shapeModule.directionRandomness);
                            localDirection = RotateVector(new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle)), baseAngle);
                        }
                        break;
                }
            }
            
            // 2. 부모로부터 상속받은 기준점에 Shape 오프셋 적용
            Vector2 worldPosition = emission.BasePosition + localPosition;
            
            // 3. 상속 플래그에 따른 속성 적용
            float emissionNormalizedTime = emission.Duration > 0 ? (emission.PlayTime / emission.Duration) : 0f;
            float speed = _mainModule.startSpeed.Evaluate(emissionNormalizedTime, UnityEngine.Random.value);
            Vector2 velocity = localDirection * speed;
            if ((emission.InheritFlags & SubEmitterInherit.Velocity) != 0)
            {
                velocity += emission.InheritedVelocity;
            }
            
            Color color = _mainModule.startColor.Evaluate(emissionNormalizedTime, UnityEngine.Random.value);
            if ((emission.InheritFlags & SubEmitterInherit.Color) != 0)
            {
                color *= emission.InheritedColor;
            }
            
            float size = _mainModule.startSize.Evaluate(emissionNormalizedTime, UnityEngine.Random.value);
            if ((emission.InheritFlags & SubEmitterInherit.Size) != 0)
            {
                // Size는 부모 기본 크기 대비 비율로 적용
                size *= emission.InheritedSizeRatio;
            }
            
            float rotation = _mainModule.startRotation.Evaluate(emissionNormalizedTime, UnityEngine.Random.value) * Mathf.Deg2Rad;
            if ((emission.InheritFlags & SubEmitterInherit.Rotation) != 0)
            {
                rotation += emission.InheritedRotation;
            }
            
            float lifetime = _mainModule.startLifetime.Evaluate(emissionNormalizedTime, UnityEngine.Random.value);
            float rotSpeed = _mainModule.angularVelocity.Evaluate(emissionNormalizedTime, UnityEngine.Random.value) * Mathf.Deg2Rad;
            
            // NoiseOffset
            float noiseOffsetX = (UnityEngine.Random.value - 0.5f) * 2000f;
            float noiseOffsetY = (UnityEngine.Random.value - 0.5f) * 2000f;
            
            // 파티클 ID 생성
            uint particleId = ++_particleIdCounter;

            // 5. 파티클 생성 (빈 슬롯 검색)
            for (int i = 0; i < _particles.Length; i++)
            {
                int index = (_lastFreeIndex + i) % _particles.Length;
                if (!_particles[index].IsAlive)
                {
                    float4 colorValue = (float4)(Vector4)color;
                    _particles[index] = new ParticleData
                    {
                        Id = particleId,
                        Position = worldPosition,
                        Velocity = velocity,
                        Color = colorValue,
                        StartColor = colorValue,
                        Size = size,
                        StartSize = size,
                        Rotation = rotation,
                        RotationSpeed = rotSpeed,
                        Lifetime = 0f,
                        MaxLifetime = lifetime,
                        IsAlive = true,
                        SimulatedPosition = worldPosition,
                        NoiseOffset = new float2(noiseOffsetX, noiseOffsetY),
                        Seed = 0,
                        SpriteIndex = -1, // 기본 파티클
                        UVRect = new float4(0, 0, 1, 1)
                    };
                    _lastFreeIndex = (index + 1) % _particles.Length;
                    
                    // Sub Emitter로 생성된 파티클은 더 이상 SubEmitter 트리거 안함 (depth 제한)
                    // _subEmitterDepth > 0 이므로 자동으로 트리거 안됨
                    
                    break;
                }
            }
        }

        private void EmitSingleInternal()
        {
            Vector2 position = Vector2.zero;
            Vector2 direction = Vector2.up;

            if (_shapeModule.enabled)
            {
                // 기준 Transform 위치 계산
                if (_shapeModule.pivot != null)
                {
                    Vector2 localPivot = transform.InverseTransformPoint(_shapeModule.pivot.position);
                    position = localPivot;
                }

                float baseAngle = _shapeModule.emissionAngle * Mathf.Deg2Rad;
                Vector2 shapeOffset = Vector2.zero;
                float spread = GetSpread();

                switch (_shapeModule.shape)
                {
                    case EmissionShape.Circle:
                        {
                            // 중앙으로부터 각도 스프레드 (0~360도)
                            float angle = spread * 2f * Mathf.PI;
                            
                            // positionRandomness: 0=중앙, 1=랜덤 반지름
                            float radius = Mathf.Lerp(0f, UnityEngine.Random.Range(0f, _shapeModule.radius), _shapeModule.positionRandomness);
                            
                            shapeOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                            shapeOffset = RotateVector(shapeOffset, baseAngle);
                            
                            // directionRandomness: 0=위치 각도, 1=랜덤 방향
                            float dirAngle = Mathf.Lerp(angle, UnityEngine.Random.Range(0f, 2f * Mathf.PI), _shapeModule.directionRandomness);
                            direction = RotateVector(new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle)), baseAngle);
                        }
                        break;

                    case EmissionShape.Cone:
                        {
                            // 중앙으로부터 각도 스프레드 (Cone 각도 범위 내)
                            float angleOffset = (spread - 0.5f) * _shapeModule.angle * Mathf.Deg2Rad;
                            
                            // positionRandomness: 0=중앙, 1=랜덤 반지름
                            float radius = Mathf.Lerp(0f, UnityEngine.Random.Range(0f, _shapeModule.radius), _shapeModule.positionRandomness);
                            
                            float totalAngle = baseAngle + angleOffset;
                            shapeOffset = new Vector2(Mathf.Cos(totalAngle), Mathf.Sin(totalAngle)) * radius;
                            
                            // directionRandomness: 0=위치 각도, 1=랜덤 방향 (Cone 범위 내)
                            float halfAngle = _shapeModule.angle * 0.5f * Mathf.Deg2Rad;
                            float randomAngleOffset = UnityEngine.Random.Range(-halfAngle, halfAngle);
                            float dirAngle = Mathf.Lerp(totalAngle, baseAngle + randomAngleOffset, _shapeModule.directionRandomness);
                            direction = new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle));
                        }
                        break;

                    case EmissionShape.Edge:
                        {
                            // 라인 위치 스프레드
                            float edgePos = (spread - 0.5f) * _shapeModule.size.x;
                            
                            // emissionAngle을 고려하여 회전
                            Vector2 edgePerp = new Vector2(-Mathf.Sin(baseAngle), Mathf.Cos(baseAngle));
                            shapeOffset = edgePerp * edgePos;
                            
                            // directionRandomness: 0=emissionAngle 방향, 1=랜덤 방향
                            float dirAngle = Mathf.Lerp(baseAngle, UnityEngine.Random.Range(0f, 2f * Mathf.PI), _shapeModule.directionRandomness);
                            direction = new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle));
                        }
                        break;

                    case EmissionShape.Rectangle:
                        {
                            // 중앙으로부터 각도 스프레드
                            float angle = spread * 2f * Mathf.PI;
                            float halfWidth = _shapeModule.size.x * 0.5f;
                            float halfHeight = _shapeModule.size.y * 0.5f;
                            float maxDistance = Mathf.Min(halfWidth, halfHeight);
                            
                            // positionRandomness: 0=중앙, 1=랜덤 거리
                            float distance = Mathf.Lerp(0f, UnityEngine.Random.Range(0f, maxDistance), _shapeModule.positionRandomness);
                            
                            shapeOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                            shapeOffset = RotateVector(shapeOffset, baseAngle);
                            
                            // directionRandomness: 0=위치 각도, 1=랜덤 방향
                            float dirAngle = Mathf.Lerp(angle, UnityEngine.Random.Range(0f, 2f * Mathf.PI), _shapeModule.directionRandomness);
                            direction = RotateVector(new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle)), baseAngle);
                        }
                        break;
                }

                position += shapeOffset;
            }
            
            // MainModule에서 속성 계산
            float normalizedTime = _mainModule.duration > 0 ? (_playTime / _mainModule.duration) : 0f;
            float speed = _mainModule.startSpeed.Evaluate(normalizedTime, UnityEngine.Random.value);
            Vector2 velocity = direction * speed;
            float size = _mainModule.startSize.Evaluate(normalizedTime, UnityEngine.Random.value);
            float lifetime = _mainModule.startLifetime.Evaluate(normalizedTime, UnityEngine.Random.value);
            Color color = _mainModule.startColor.Evaluate(normalizedTime, UnityEngine.Random.value);
            float rotation = _mainModule.startRotation.Evaluate(normalizedTime, UnityEngine.Random.value) * Mathf.Deg2Rad;
            float rotSpeed = _mainModule.angularVelocity.Evaluate(normalizedTime, UnityEngine.Random.value) * Mathf.Deg2Rad;
            
            // NoiseOffset
            float noiseOffsetX = (UnityEngine.Random.value - 0.5f) * 2000f;
            float noiseOffsetY = (UnityEngine.Random.value - 0.5f) * 2000f;
            
            // 파티클 ID 생성
            uint particleId = ++_particleIdCounter;

            // 빈 슬롯 검색
            for (int i = 0; i < _particles.Length; i++)
            {
                int index = (_lastFreeIndex + i) % _particles.Length;
                if (!_particles[index].IsAlive)
                {
                    float4 colorValue = (float4)(Vector4)color;
                    _particles[index] = new ParticleData
                    {
                        Id = particleId,
                        Position = position,
                        Velocity = velocity,
                        Color = colorValue,
                        StartColor = colorValue,
                        Size = size,
                        StartSize = size,
                        Rotation = rotation,
                        RotationSpeed = rotSpeed,
                        Lifetime = 0f,
                        MaxLifetime = lifetime,
                        IsAlive = true,
                        SimulatedPosition = position,
                        NoiseOffset = new float2(noiseOffsetX, noiseOffsetY),
                        Seed = 0,
                        SpriteIndex = -1, // 기본 파티클
                        UVRect = new float4(0, 0, 1, 1)
                    };
                    _lastFreeIndex = (index + 1) % _particles.Length;
                    
                    // Birth Sub Emitter 트리거 (최상위 시스템만)
                    if (_subEmitterModule.enabled && _subEmitterDepth == 0)
                    {
                        TriggerSubEmitters(SubEmitterType.Birth, index);
                    }
                    
                    break;
                }
            }
        }
        
        #region Sub Emitter
        
        /// <summary>
        /// Sub Emitter 트리거 - 자식 시스템에 ParticleEmission 추가
        /// </summary>
        private void TriggerSubEmitters(SubEmitterType type, int parentIndex)
        {
            if (!_subEmitterModule.enabled) return;
            
            foreach (var entry in _subEmitterModule.subEmitters)
            {
                if (entry.type != type || entry.subParticleSystem == null) continue;
                
                var childSystem = entry.subParticleSystem;
                
                // 순환 참조 체크 (자기 자신 참조 방지)
                if (childSystem == this) continue;
                
                // 부모 파티클 정보 수집
                Vector2 position = GetParticlePosition(parentIndex);
                Vector2 velocity = GetParticleVelocity(parentIndex) * entry.inheritVelocityMultiplier;
                Color color = GetParticleColor(parentIndex);
                
                // Size: 부모 기본 크기 대비 현재 파티클 크기의 비율
                float parentNormalizedTime = _mainModule.duration > 0 ? (_playTime / _mainModule.duration) : 0f;
                float parentStartSize = _mainModule.startSize.Evaluate(parentNormalizedTime, 0.5f); // 부모 기본 크기
                float currentSize = GetParticleSize(parentIndex);
                float sizeRatio = parentStartSize > 0 ? (currentSize / parentStartSize) : 1f;
                float inheritedSizeRatio = sizeRatio * entry.inheritSizeMultiplier;
                
                float rotation = GetParticleRotation(parentIndex);
                uint particleId = GetParticleId(parentIndex);
                
                // ParticleEmission 생성
                var emission = new ParticleEmission(
                    childSystem._nextEmissionId++,
                    type,
                    this,           // 부모 시스템
                    parentIndex,
                    particleId,
                    position,
                    velocity,
                    color,
                    inheritedSizeRatio,
                    rotation,
                    entry.inherit,
                    childSystem._mainModule.duration,
                    childSystem._mainModule.loop
                );
                
                // 자식 시스템에 세션 추가
                childSystem.AddSubEmission(emission);
            }
        }
        
        /// <summary>
        /// Sub Emitter 세션 추가 (부모 시스템이 호출)
        /// </summary>
        internal void AddSubEmission(ParticleEmission emission)
        {
            // depth 설정 (Sub Emitter로 작동 중임을 표시)
            _subEmitterDepth = 1;
            
            // Burst 상태 초기화 (이 시스템의 EmissionModule 설정 사용)
            foreach (var burstEntry in _emissionModule.bursts)
            {
                emission.BurstStates.Add(new BurstState 
                { 
                    BurstEntry = burstEntry, 
                    CurrentCycle = 0, 
                    LastBurstTime = -1f 
                });
            }
            
            _emissions.Add(emission);
        }
        
        /// <summary>
        /// 주어진 파티클 시스템이 이 시스템의 SubEmitter로 등록되어 있는지 확인
        /// </summary>
        private bool IsSubEmitterChild(CanvasParticleSystem childSystem)
        {
            if (!_subEmitterModule.enabled) return false;
            
            foreach (var entry in _subEmitterModule.subEmitters)
            {
                if (entry.subParticleSystem == childSystem)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        #endregion

        private Vector2 RotateVector(Vector2 v, float angleRad)
        {
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        /// <summary>
        /// SpreadMode에 따라 0~1 범위의 값을 반환
        /// </summary>
        private float GetSpreadValue(SpreadMode mode, ref float loopProgress, ref bool pingPongReverse, float speed, int burstCount = 0)
        {
            switch (mode)
            {
                case SpreadMode.Random:
                    return UnityEngine.Random.value;

                case SpreadMode.Loop:
                    {
                        float result = loopProgress;
                        loopProgress = (loopProgress + speed * 0.01f) % 1f;
                        return result;
                    }

                case SpreadMode.PingPong:
                    {
                        float result = loopProgress;
                        
                        if (pingPongReverse)
                        {
                            loopProgress -= speed * 0.01f;
                            if (loopProgress <= 0f)
                            {
                                loopProgress = 0f;
                                pingPongReverse = false;
                            }
                        }
                        else
                        {
                            loopProgress += speed * 0.01f;
                            if (loopProgress >= 1f)
                            {
                                loopProgress = 1f;
                                pingPongReverse = true;
                            }
                        }
                        
                        return result;
                    }

                case SpreadMode.BurstSpread:
                    if (burstCount > 1)
                    {
                        int burstIndex = (int)(loopProgress * 1000f) % burstCount;
                        loopProgress = (loopProgress + 0.001f) % 1f;
                        return (float)burstIndex / (burstCount - 1);
                    }
                    return 0.5f; // 단일 파티클

                default:
                    return UnityEngine.Random.value;
            }
        }

        /// <summary>
        /// 스프레드 값 계산 (0~1)
        /// </summary>
        private float GetSpread(int burstCount = 0)
        {
            return GetSpreadValue(
                _shapeModule.spreadMode,
                ref _shapeModule.spreadLoopProgress,
                ref _shapeModule.spreadPingPongReverse,
                _shapeModule.spreadSpeed,
                burstCount
            );
        }

        private void UpdateParticles(float deltaTime)
        {
            float2 attractionTarget = float2.zero;
            bool hasAttractionTarget = _attractionModule.enabled && _attractionModule.target != null;

            if (hasAttractionTarget)
            {
                Vector2 localTarget = transform.InverseTransformPoint(_attractionModule.target!.position);
                attractionTarget = new float2(localTarget.x, localTarget.y);
            }

            float2 noiseScrollSpeed = new float2(_noiseModule.scrollSpeed.x, _noiseModule.scrollSpeed.y);

            for (int i = 0; i < _particles.Length; i++)
            {
                if (!_particles[i].IsAlive) continue;

                var particle = _particles[i];

                particle.Lifetime += deltaTime;
                if (particle.Lifetime >= particle.MaxLifetime)
                {
                    // Death Sub Emitter 트리거 (죽기 직전에)
                    if (_subEmitterModule.enabled && _subEmitterDepth == 0)
                    {
                        TriggerSubEmitters(SubEmitterType.Death, i);
                    }
                    
                    // 렌더러에서 파티클 제거
                    if (particle.SpriteIndex >= 0)
                    {
                        var renderer = _rendererPool?.FindRendererByIndex(particle.SpriteIndex);
                        renderer?.RemoveParticle(i);
                    }
                    
                    particle.IsAlive = false;
                    _particles[i] = particle;
                    continue;
                }

                float t = particle.MaxLifetime > 0 ? (particle.Lifetime / particle.MaxLifetime) : 1f;
                uint seed = particle.Seed;

                // Velocity Over Lifetime
                if (_velocityOverLifetime.enabled)
                {
                    // X/Y 속도 추가
                    particle.Velocity.x += _velocityOverLifetime.velocityX.Evaluate(t, 0f) * deltaTime;
                    particle.Velocity.y += _velocityOverLifetime.velocityY.Evaluate(t, 0f) * deltaTime;
                }

                particle.SimulatedPosition += particle.Velocity * deltaTime;
                
                // Noise (시드 + 시간 기반 결정론적 노이즈)
                if (_noiseModule.enabled)
                {
                    float strength = _noiseModule.strength.Evaluate(t, 0f);
                    float freq = _noiseModule.frequency;
                    
                    // 시드 기반 노이즈 샘플링 위치
                    float2 samplePos = particle.NoiseOffset + noiseScrollSpeed * particle.Lifetime;
                    samplePos *= freq;
                    
                    // Perlin noise 샘플링
                    float2 noiseValue = new float2(
                        noise.cnoise(new float2(samplePos.x, samplePos.y)),
                        noise.cnoise(new float2(samplePos.x + 100f, samplePos.y + 100f))
                    );
                    
                    particle.SimulatedPosition += noiseValue * strength * deltaTime;
                }

                // Attraction
                if (hasAttractionTarget)
                {
                    float amount = _attractionModule.amount.Evaluate(t, 0f);
                    particle.Position = math.lerp(particle.SimulatedPosition, attractionTarget, math.saturate(amount));
                }
                else
                {
                    particle.Position = particle.SimulatedPosition;
                }

                particle.Rotation += particle.RotationSpeed * deltaTime;

                if (_sizeOverLifetime.enabled) particle.Size = particle.StartSize * _sizeOverLifetime.size.Evaluate(t, 0f);
                if (_colorOverLifetime.enabled)
                {
                    Color color = _colorOverLifetime.color.Evaluate(t, 0f);
                    // StartColor와 ColorOverLifetime 곱셈
                    float4 gradientColor = (float4)(Vector4)color;
                    particle.Color = particle.StartColor * gradientColor;
                }

                _particles[i] = particle;
            }
        }

        private Color EvaluateColor(ParticleSystem.MinMaxGradient gradient, float time)
        {
            return gradient.Evaluate(time);
        }

        private float EvaluateCurve(ParticleSystem.MinMaxCurve curve, float time)
        {
            return curve.Evaluate(time);
        }

        /// <summary>
        /// 버스트 상태를 리셋합니다.
        /// </summary>
        /// <summary>
        /// 파티클 시스템을 재생합니다.
        /// </summary>
        /// <param name="playChildren">하위 파티클 시스템도 함께 재생할지 여부 (에디터 동작)</param>
        public void Play(bool playChildren = true)
        {
            if (!_isInitialized)
                Initialize();

            _isPlaying = true;
            _isPaused = false;
            _lastUpdateTime = Time.realtimeSinceStartup;

            // 자체 방출 상태 리셋
            _playTime = 0f;
            _emissionAccumulator = 1f; // 시작 시 즉시 1개 방출되도록
            _burstStates.Clear();
            foreach (var burstEntry in _emissionModule.bursts)
            {
                _burstStates.Add(new BurstState 
                { 
                    BurstEntry = burstEntry, 
                    CurrentCycle = 0, 
                    LastBurstTime = -1f 
                });
            }

#if UNITY_EDITOR
            // 에디터에서는 하위 파티클 시스템도 함께 재생 (Unity 동작 모방)
            // 단, SubEmitter의 자식으로 등록된 시스템은 제외
            if (playChildren && !Application.isPlaying)
            {
                var childSystems = GetComponentsInChildren<CanvasParticleSystem>(true);
                foreach (var childSystem in childSystems)
                {
                    if (childSystem != this && childSystem != null)
                    {
                        // SubEmitter로 등록된 자식은 수동 플레이 방지
                        if (!IsSubEmitterChild(childSystem))
                        {
                            childSystem.PlayInternal(playChildren: false); // 재귀 방지
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        /// 내부 플레이 (재귀 방지용) - 하위 시스템 플레이용
        /// </summary>
        private void PlayInternal(bool playChildren)
        {
            if (!_isInitialized)
                Initialize();

            _isPlaying = true;
            _isPaused = false;
            _isManuallyPaused = false;
            _lastUpdateTime = Time.realtimeSinceStartup;

            // 자체 방출 상태 리셋
            _playTime = 0f;
            _emissionAccumulator = 1f; // 시작 시 즉시 1개 방출되도록
            _burstStates.Clear();
            foreach (var burstEntry in _emissionModule.bursts)
            {
                _burstStates.Add(new BurstState 
                { 
                    BurstEntry = burstEntry, 
                    CurrentCycle = 0, 
                    LastBurstTime = -1f 
                });
            }
        }
        
        /// <summary>
        /// Sub Emitter 자식 시스템들을 일시정지
        /// </summary>
        private void PauseSubEmitterChildren()
        {
            if (!_subEmitterModule.enabled) return;
            
            foreach (var entry in _subEmitterModule.subEmitters)
            {
                if (entry.subParticleSystem != null && entry.subParticleSystem != this)
                {
                    entry.subParticleSystem._isPaused = true;
                }
            }
        }
        
        /// <summary>
        /// Sub Emitter 자식 시스템들을 재개
        /// </summary>
        private void ResumeSubEmitterChildren()
        {
            if (!_subEmitterModule.enabled) return;
            
            foreach (var entry in _subEmitterModule.subEmitters)
            {
                if (entry.subParticleSystem != null && entry.subParticleSystem != this)
                {
                    entry.subParticleSystem._isPaused = false;
                    entry.subParticleSystem._lastUpdateTime = Time.realtimeSinceStartup;
                }
            }
        }

        /// <summary>
        /// 시뮬레이션을 일시 정지합니다.
        /// </summary>
        public void Pause()
        {
            if (_isPlaying)
            {
                _isPaused = true;
                _isManuallyPaused = true;
                PauseSubEmitterChildren();
            }
        }

        /// <summary>
        /// 일시 정지된 시뮬레이션을 재개합니다.
        /// </summary>
        public void Resume()
        {
            if (_isPlaying && _isPaused)
            {
                _isPaused = false;
                _isManuallyPaused = false;
                _lastUpdateTime = Time.realtimeSinceStartup;
                ResumeSubEmitterChildren();
            }
        }

        /// <summary>
        /// 시뮬레이션을 정지하고 모든 파티클을 제거합니다.
        /// </summary>
        /// <param name="clearParticles">파티클도 즉시 제거할지 여부 (기본: true)</param>
        /// <param name="stopChildren">하위 파티클 시스템도 함께 정지할지 여부</param>
        public void Stop(bool clearParticles = true, bool stopChildren = true)
        {
            _isPlaying = false;
            _isPaused = false;
            _isManuallyPaused = false;
            
            // SubEmitter 세션만 제거
            _emissions.RemoveAll(e => e.SourceType == EmissionSourceType.SubEmitter);
            
            // Sub Emitter depth 초기화
            _subEmitterDepth = 0;
            
            // 자체 방출 상태 리셋
            _playTime = 0f;
            _emissionAccumulator = 0f;
            _burstStates.Clear();
            
            // 기본적으로 파티클 제거
            if (clearParticles)
            {
                _simulationTime = 0f;
                Clear();
            }

#if UNITY_EDITOR
            // 에디터에서는 하위 파티클 시스템도 함께 정지
            if (stopChildren && !Application.isPlaying)
            {
                var childSystems = GetComponentsInChildren<CanvasParticleSystem>(true);
                foreach (var childSystem in childSystems)
                {
                    if (childSystem != this && childSystem != null)
                    {
                        childSystem.Stop(clearParticles, stopChildren: false); // 재귀 방지
                    }
                }
            }
#endif
        }

        /// <summary>
        /// 시뮬레이션을 재시작합니다.
        /// </summary>
        public void Restart()
        {
            Stop(clearParticles: true);
            Play();
        }

        /// <summary>
        /// 지정된 시간만큼 시뮬레이션을 진행합니다. (에디터 미리보기용)
        /// </summary>
        public void Simulate(float time, bool restart = false)
        {
            if (restart)
            {
                Clear();
                _simulationTime = 0f;
                _emissions.Clear();
                _nextEmissionId = 0;
            }

            if (!_isInitialized)
                Initialize();

            // 작은 단위로 나누어 시뮬레이션 (물리 정확도를 위해)
            float stepTime = 0.02f; // 50fps
            while (time > 0f)
            {
                float dt = Mathf.Min(time, stepTime);
                SimulateInternal(dt);
                time -= dt;
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (!_isInitialized)
                return;

            if (_activeParticleCount == 0)
                return;

            // 기본 파티클만 렌더링 (SpriteIndex = -1)
            for (int i = 0; i < _particles.Length; i++)
            {
                if (!_particles[i].IsAlive)
                    continue;

                if (_particles[i].SpriteIndex >= 0)
                    continue; // Emit(Sprite)로 생성된 파티클은 서브 렌더러가 담당

                AddParticleQuad(vh, _particles[i]);
            }
        }

        private void AddParticleQuad(VertexHelper vh, ParticleData particle)
        {
            int vertexIndex = vh.currentVertCount;

            // 회전 및 스케일 적용
            float halfSize = particle.Size * 0.5f;
            float cos = math.cos(particle.Rotation);
            float sin = math.sin(particle.Rotation);

            // 4개의 정점 위치 계산 (할당 제거)
            Vector2 v0 = RotatePoint(new float2(-halfSize, -halfSize), cos, sin);
            Vector2 v1 = RotatePoint(new float2(-halfSize, halfSize), cos, sin);
            Vector2 v2 = RotatePoint(new float2(halfSize, halfSize), cos, sin);
            Vector2 v3 = RotatePoint(new float2(halfSize, -halfSize), cos, sin);

            // 파티클 위치 적용
            Vector3 pos = new Vector3(particle.Position.x, particle.Position.y, 0);
            Color32 color = new Color(particle.Color.x, particle.Color.y, particle.Color.z, particle.Color.w);

            // UV 좌표 계산
            Vector2 uv0, uv1, uv2, uv3;
            CalculateUVs(particle, out uv0, out uv1, out uv2, out uv3);

            // 정점 추가
            vh.AddVert(pos + (Vector3)v0, color, uv0);
            vh.AddVert(pos + (Vector3)v1, color, uv1);
            vh.AddVert(pos + (Vector3)v2, color, uv2);
            vh.AddVert(pos + (Vector3)v3, color, uv3);

            // 인덱스 추가 (2개의 삼각형)
            vh.AddTriangle(vertexIndex + 0, vertexIndex + 1, vertexIndex + 2);
            vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex + 0);
        }

        private void CalculateUVs(ParticleData particle, out Vector2 uv0, out Vector2 uv1, out Vector2 uv2, out Vector2 uv3)
        {
            // 기본 UV 범위 (스프라이트 기반)
            float baseUVMinX = 0f;
            float baseUVMinY = 0f;
            float baseUVMaxX = 1f;
            float baseUVMaxY = 1f;
            
            // mainSprite가 있으면 해당 스프라이트의 UV 범위 사용
            if (_rendererModule.mainSprite != null && _rendererModule.mainSprite.texture != null)
            {
                var sprite = _rendererModule.mainSprite;
                var rect = sprite.textureRect;
                float texWidth = sprite.texture.width;
                float texHeight = sprite.texture.height;
                
                baseUVMinX = rect.x / texWidth;
                baseUVMinY = rect.y / texHeight;
                baseUVMaxX = (rect.x + rect.width) / texWidth;
                baseUVMaxY = (rect.y + rect.height) / texHeight;
            }
            
            if (!_textureSheetAnimation.enabled)
            {
                uv0 = new Vector2(baseUVMinX, baseUVMinY);
                uv1 = new Vector2(baseUVMinX, baseUVMaxY);
                uv2 = new Vector2(baseUVMaxX, baseUVMaxY);
                uv3 = new Vector2(baseUVMaxX, baseUVMinY);
                return;
            }

            // 프레임 범위 계산
            int totalFrames = _textureSheetAnimation.tilesX * _textureSheetAnimation.tilesY;
            int startFrame = Mathf.Clamp(_textureSheetAnimation.startFrame, 0, totalFrames - 1);
            int endFrame = _textureSheetAnimation.endFrame < 0 ? totalFrames - 1 : Mathf.Clamp(_textureSheetAnimation.endFrame, startFrame, totalFrames - 1);
            int frameCount = endFrame - startFrame + 1;

            float normalizedTime;
            if (_textureSheetAnimation.mode == TextureSheetAnimationMode.Lifetime)
            {
                // Lifetime 모드: 수명 비율 기반
                float t = particle.MaxLifetime > 0 ? (particle.Lifetime / particle.MaxLifetime) : 0f;
                normalizedTime = t * frameCount * _textureSheetAnimation.cycles;
            }
            else
            {
                // FPS 모드: 절대 시간 기반
                normalizedTime = particle.Lifetime * _textureSheetAnimation.fps;
            }

            int frameIndex = Mathf.FloorToInt(normalizedTime) % frameCount;
            int currentFrame = startFrame + frameIndex;

            int frameX = currentFrame % _textureSheetAnimation.tilesX;
            int frameY = _textureSheetAnimation.tilesY - 1 - (currentFrame / _textureSheetAnimation.tilesX);

            // 스프라이트 UV 범위 내에서 타일 UV 계산
            float spriteUVWidth = baseUVMaxX - baseUVMinX;
            float spriteUVHeight = baseUVMaxY - baseUVMinY;
            
            float tileUVWidth = spriteUVWidth / _textureSheetAnimation.tilesX;
            float tileUVHeight = spriteUVHeight / _textureSheetAnimation.tilesY;

            float uvX = baseUVMinX + frameX * tileUVWidth;
            float uvY = baseUVMinY + frameY * tileUVHeight;

            uv0 = new Vector2(uvX, uvY);
            uv1 = new Vector2(uvX, uvY + tileUVHeight);
            uv2 = new Vector2(uvX + tileUVWidth, uvY + tileUVHeight);
            uv3 = new Vector2(uvX + tileUVWidth, uvY);
        }

        private Vector2 RotatePoint(float2 point, float cos, float sin)
        {
            return new Vector2(
                point.x * cos - point.y * sin,
                point.x * sin + point.y * cos
            );
        }

        /// <summary>
        /// 파티클을 방출합니다.
        /// </summary>
        public void Emit(Vector2 position, Vector2 velocity, Color color, float size, float lifetime, float rotation = 0f, float rotationSpeed = 0f)
        {
            if (!_isInitialized)
                Initialize();

            // 파티클 ID 생성
            uint particleId = ++_particleIdCounter;
            
            // 비활성 파티클 찾기
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i].IsAlive)
                    continue;

                float4 colorValue = new float4(color.r, color.g, color.b, color.a);
                _particles[i] = new ParticleData
                {
                    Id = particleId,
                    Position = position,
                    Velocity = velocity,
                    Color = colorValue,
                    StartColor = colorValue,
                    Size = size,
                    StartSize = size,
                    Rotation = rotation,
                    RotationSpeed = rotationSpeed,
                    Lifetime = 0f,
                    MaxLifetime = lifetime,
                    IsAlive = true,
                    SimulatedPosition = position,
                    SpriteIndex = -1,
                    UVRect = new float4(0, 0, 1, 1)
                };
                
                // Birth Sub Emitter 트리거 (최상위 시스템만)
                if (_subEmitterModule.enabled && _subEmitterDepth == 0)
                {
                    TriggerSubEmitters(SubEmitterType.Birth, i);
                }
                
                return;
            }
        }

        /// <summary>
        /// 모듈 설정을 기반으로 파티클 하나를 방출합니다.
        /// </summary>
        public void EmitSingle()
        {
            if (!_isInitialized) Initialize();
            EmitSingleInternal();
        }

        /// <summary>
        /// 지정된 개수만큼 파티클을 한꺼번에 방출합니다.
        /// </summary>
        public void EmitBurst(int count)
        {
            if (!_isInitialized) Initialize();
            for (int i = 0; i < count; i++)
            {
                EmitSingleInternal();
            }
        }

        /// <summary>
        /// 지정된 스프라이트로 파티클 1개를 방출합니다.
        /// </summary>
        /// <param name="sprite">파티클에 적용할 스프라이트</param>
        public void Emit(Sprite sprite)
        {
            Emit(sprite, 1);
        }

        /// <summary>
        /// 지정된 스프라이트로 파티클을 여러 개 방출합니다.
        /// </summary>
        /// <param name="sprite">파티클에 적용할 스프라이트</param>
        /// <param name="count">방출할 파티클 개수</param>
        public void Emit(Sprite sprite, int count)
        {
            if (!_isInitialized) Initialize();
            
            if (_rendererPool == null)
            {
                _rendererPool = new ParticleRendererPool(this, _rendererModule.maxRendererPoolSize);
            }

            // 스프라이트에 대한 렌더러 가져오기
            var renderer = _rendererPool.GetRenderer(sprite);
            int spriteIndex = renderer.SpriteIndex;

            for (int i = 0; i < count; i++)
            {
                EmitWithSprite(sprite, spriteIndex, renderer);
            }
        }

        /// <summary>
        /// 지정된 스프라이트로 파티클을 일정 간격으로 방출합니다.
        /// </summary>
        /// <param name="sprite">파티클에 적용할 스프라이트</param>
        /// <param name="count">방출할 파티클 개수</param>
        /// <param name="interval">방출 간격 (초)</param>
        public void Emit(Sprite sprite, int count, float interval)
        {
            if (count <= 0) return;
            
            if (interval <= 0f)
            {
                Emit(sprite, count);
                return;
            }

            StartCoroutine(EmitWithIntervalCoroutine(sprite, count, interval));
        }

        private System.Collections.IEnumerator EmitWithIntervalCoroutine(Sprite sprite, int count, float interval)
        {
            for (int i = 0; i < count; i++)
            {
                Emit(sprite, 1);
                yield return new WaitForSeconds(interval);
            }
        }

        /// <summary>
        /// 스프라이트가 지정된 파티클 방출 (내부용)
        /// </summary>
        private void EmitWithSprite(Sprite sprite, int spriteIndex, CanvasParticleRenderer renderer)
        {
            Vector2 position = Vector2.zero;
            Vector2 direction = Vector2.up;

            // Shape 모듈로 위치/방향 계산
            if (_shapeModule.enabled)
            {
                if (_shapeModule.pivot != null)
                {
                    Vector2 localPivot = transform.InverseTransformPoint(_shapeModule.pivot.position);
                    position = localPivot;
                }

                float baseAngle = _shapeModule.emissionAngle * Mathf.Deg2Rad;
                Vector2 shapeOffset = Vector2.zero;
                float spread = GetSpread();

                switch (_shapeModule.shape)
                {
                    case EmissionShape.Circle:
                        {
                            float angle = spread * 2f * Mathf.PI;
                            float radius = Mathf.Lerp(0f, UnityEngine.Random.Range(0f, _shapeModule.radius), _shapeModule.positionRandomness);
                            shapeOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                            shapeOffset = RotateVector(shapeOffset, baseAngle);
                            float dirAngle = Mathf.Lerp(angle, UnityEngine.Random.Range(0f, 2f * Mathf.PI), _shapeModule.directionRandomness);
                            direction = RotateVector(new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle)), baseAngle);
                        }
                        break;

                    case EmissionShape.Cone:
                        {
                            float angleOffset = (spread - 0.5f) * _shapeModule.angle * Mathf.Deg2Rad;
                            float radius = Mathf.Lerp(0f, UnityEngine.Random.Range(0f, _shapeModule.radius), _shapeModule.positionRandomness);
                            float totalAngle = baseAngle + angleOffset;
                            shapeOffset = new Vector2(Mathf.Cos(totalAngle), Mathf.Sin(totalAngle)) * radius;
                            float halfAngle = _shapeModule.angle * 0.5f * Mathf.Deg2Rad;
                            float randomAngleOffset = UnityEngine.Random.Range(-halfAngle, halfAngle);
                            float dirAngle = Mathf.Lerp(totalAngle, baseAngle + randomAngleOffset, _shapeModule.directionRandomness);
                            direction = new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle));
                        }
                        break;

                    case EmissionShape.Edge:
                        {
                            float t = spread;
                            shapeOffset = new Vector2((t - 0.5f) * _shapeModule.size.x, 0f);
                            shapeOffset = RotateVector(shapeOffset, baseAngle);
                            float dirAngle = Mathf.Lerp(baseAngle, UnityEngine.Random.Range(0f, 2f * Mathf.PI), _shapeModule.directionRandomness);
                            direction = new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle));
                        }
                        break;

                    case EmissionShape.Rectangle:
                        {
                            float angleOffset = (spread - 0.5f) * 2f * Mathf.PI;
                            Vector2 maxOffset = new Vector2(_shapeModule.size.x * 0.5f, _shapeModule.size.y * 0.5f);
                            float rx = UnityEngine.Random.Range(-maxOffset.x, maxOffset.x);
                            float ry = UnityEngine.Random.Range(-maxOffset.y, maxOffset.y);
                            shapeOffset = Vector2.Lerp(Vector2.zero, new Vector2(rx, ry), _shapeModule.positionRandomness);
                            shapeOffset = RotateVector(shapeOffset, baseAngle);
                            float totalAngle = baseAngle + angleOffset;
                            float dirAngle = Mathf.Lerp(totalAngle, UnityEngine.Random.Range(0f, 2f * Mathf.PI), _shapeModule.directionRandomness);
                            direction = new Vector2(Mathf.Cos(dirAngle), Mathf.Sin(dirAngle));
                        }
                        break;
                }

                position += shapeOffset;
            }

            // MainModule에서 속성 계산
            float normalizedTime = _mainModule.duration > 0 ? (_playTime / _mainModule.duration) : 0f;
            float speed = _mainModule.startSpeed.Evaluate(normalizedTime, UnityEngine.Random.value);
            Vector2 velocity = direction * speed;
            float size = _mainModule.startSize.Evaluate(normalizedTime, UnityEngine.Random.value);
            float lifetime = _mainModule.startLifetime.Evaluate(normalizedTime, UnityEngine.Random.value);
            Color color = _mainModule.startColor.Evaluate(normalizedTime, UnityEngine.Random.value);
            float rotation = _mainModule.startRotation.Evaluate(normalizedTime, UnityEngine.Random.value) * Mathf.Deg2Rad;
            float rotSpeed = _mainModule.angularVelocity.Evaluate(normalizedTime, UnityEngine.Random.value) * Mathf.Deg2Rad;
            
            // NoiseOffset
            float noiseOffsetX = (UnityEngine.Random.value - 0.5f) * 2000f;
            float noiseOffsetY = (UnityEngine.Random.value - 0.5f) * 2000f;

            // 스프라이트 UV 계산
            float4 uvRect = new float4(0, 0, 1, 1);
            if (sprite != null && sprite.texture != null)
            {
                var rect = sprite.textureRect;
                float texWidth = sprite.texture.width;
                float texHeight = sprite.texture.height;
                uvRect = new float4(
                    rect.x / texWidth,
                    rect.y / texHeight,
                    rect.width / texWidth,
                    rect.height / texHeight
                );
            }
            
            // 파티클 ID 생성
            uint particleId = ++_particleIdCounter;

            // 파티클 생성 (빈 슬롯 검색)
            for (int i = 0; i < _particles.Length; i++)
            {
                int index = (_lastFreeIndex + i) % _particles.Length;
                if (!_particles[index].IsAlive)
                {
                    float4 colorValue = new float4(color.r, color.g, color.b, color.a);
                    _particles[index] = new ParticleData
                    {
                        Id = particleId,
                        Position = position,
                        Velocity = velocity,
                        Color = colorValue,
                        StartColor = colorValue,
                        Size = size,
                        StartSize = size,
                        Rotation = rotation,
                        RotationSpeed = rotSpeed,
                        Lifetime = 0f,
                        MaxLifetime = lifetime,
                        IsAlive = true,
                        SimulatedPosition = position,
                        NoiseOffset = new float2(noiseOffsetX, noiseOffsetY),
                        Seed = 0,
                        SpriteIndex = spriteIndex,
                        UVRect = uvRect
                    };
                    _lastFreeIndex = (index + 1) % _particles.Length;
                    
                    // 렌더러에 파티클 인덱스 등록
                    renderer.AddParticle(index);
                    
                    // Birth Sub Emitter 트리거 (최상위 시스템만)
                    if (_subEmitterModule.enabled && _subEmitterDepth == 0)
                    {
                        TriggerSubEmitters(SubEmitterType.Birth, index);
                    }
                    
                    break;
                }
            }
        }

        /// <summary>
        /// 모든 파티클을 제거합니다.
        /// </summary>
        public void Clear()
        {
            if (!_isInitialized)
                return;

            for (int i = 0; i < _particles.Length; i++)
            {
                var particle = _particles[i];
                particle.IsAlive = false;
                _particles[i] = particle;
            }

            // Sprite Override 렌더러 풀 정리
            _rendererPool?.Clear();

            _activeParticleCount = 0;
            SetVerticesDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (_maxParticles < 1)
                _maxParticles = 1;

            if (_mainModule.duration < 0.01f)
                _mainModule.duration = 0.01f;

            if (_isInitialized && _particles != null && _particles.Length != _maxParticles)
            {
                Cleanup();
                Initialize();
            }
            else if (!_isInitialized)
            {
                // 에디터에서 처음 생성되거나 설정 변경 시 초기화
                Initialize();
            }
        }
#endif
    }
}
