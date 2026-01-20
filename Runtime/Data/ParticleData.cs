using Unity.Mathematics;

namespace Waker.CanvasParticleSystems
{
    /// <summary>
    /// 개별 파티클의 데이터를 저장하는 구조체
    /// Job System에서 사용하기 위해 blittable 타입만 사용
    /// </summary>
    public struct ParticleData
    {
        /// <summary>
        /// 파티클 고유 ID (인덱스 재사용 시 유효성 검증에 사용)
        /// </summary>
        public uint Id;
        
        public float2 Position;
        public float2 Velocity;
        public float4 Color;
        public float4 StartColor; // ColorOverLifetime/상속을 위한 시작 색상
        public float Size;
        public float StartSize; // SizeOverLifetime을 위한 시작 크기
        public float Rotation;
        public float RotationSpeed;
        public float Lifetime;
        public float MaxLifetime;
        public bool IsAlive;
        
        // Attraction을 위한 시작 위치 (물리 시뮬레이션 결과)
        public float2 SimulatedPosition;
        
        // Noise 모듈을 위한 오프셋
        public float2 NoiseOffset;
        
        // 파티클 고유 시드 (결정론적 노이즈/움직임)
        public uint Seed;
        
        /// <summary>
        /// 스프라이트 인덱스 (-1: 기본 렌더러 사용, 0~: 스프라이트별 렌더러 인덱스)
        /// </summary>
        public int SpriteIndex;
        
        /// <summary>
        /// 스프라이트별 UV Rect (x, y, width, height)
        /// 기본값: (0, 0, 1, 1)
        /// </summary>
        public float4 UVRect;
        
        public float NormalizedLifetime => MaxLifetime > 0 ? Lifetime / MaxLifetime : 0f;
    }
}
