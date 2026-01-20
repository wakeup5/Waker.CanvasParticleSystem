using System;
using System.Collections.Generic;
using UnityEngine;

namespace Waker.CanvasParticleSystems
{
    /// <summary>
    /// Emission 생성 소스 타입
    /// </summary>
    public enum EmissionSourceType
    {
        /// <summary>
        /// SubEmitter Birth/Death 이벤트로 생성된 세션
        /// </summary>
        SubEmitter,
    }

    /// <summary>
    /// Sub Emitter 트리거 타입
    /// </summary>
    public enum SubEmitterType
    {
        /// <summary>
        /// 부모 파티클 생성 시 - 부모 위치를 지속 추적
        /// </summary>
        Birth,
        
        /// <summary>
        /// 부모 파티클 소멸 시 - 마지막 위치에서 방출 (추적 없음)
        /// </summary>
        Death,
    }

    /// <summary>
    /// 부모 속성 상속 플래그
    /// </summary>
    [Flags]
    public enum SubEmitterInherit
    {
        None = 0,
        /// <summary>
        /// Birth: 지속 추적, Death: 고정 위치
        /// </summary>
        Position = 1 << 0,
        /// <summary>
        /// 부모 파티클의 속도 상속
        /// </summary>
        Velocity = 1 << 1,
        /// <summary>
        /// 부모 파티클의 색상 상속
        /// </summary>
        Color = 1 << 2,
        /// <summary>
        /// 부모 파티클의 크기 상속 (부모 기본 크기 대비 비율)
        /// </summary>
        Size = 1 << 3,
        /// <summary>
        /// 부모 파티클의 회전 상속
        /// </summary>
        Rotation = 1 << 4,
        /// <summary>
        /// 모든 속성 상속
        /// </summary>
        All = Position | Velocity | Color | Size | Rotation
    }

    /// <summary>
    /// 개별 Burst의 상태 추적
    /// </summary>
    public class BurstState
    {
        public BurstEntry BurstEntry { get; set; } = null!;
        public int CurrentCycle { get; set; }
        public float LastBurstTime { get; set; }
    }

    /// <summary>
    /// 파티클 방출 세션 - Play() 1회 또는 부모 파티클 1개당 생성
    /// 독립적인 시간 추적, Burst 타이밍, Rate Over Time 방출을 관리
    /// </summary>
    public class ParticleEmission
    {
        // 세션 식별
        public int Id { get; private set; }
        public EmissionSourceType SourceType { get; private set; }
        
        // 시간 추적
        public float PlayTime { get; set; }
        public float Duration { get; private set; }
        public bool IsLooping { get; private set; }
        public bool IsAlive => IsLooping || PlayTime < Duration;
        
        // Burst 상태 (각 Emission이 독립적으로 관리)
        public List<BurstState> BurstStates { get; private set; }
        
        // Rate Over Time 누적
        public float EmissionAccumulator { get; set; }
        
        // Sub Emitter 전용
        public bool IsSubEmitter => SourceType == EmissionSourceType.SubEmitter;
        
        // 부모 파티클 추적 정보
        public CanvasParticleSystem? ParentSystem { get; private set; }
        public int ParentParticleIndex { get; private set; }
        public uint ParentParticleId { get; private set; } // 파티클 고유 ID (인덱스 재사용 검증)
        
        // Sub Emitter 타입
        public SubEmitterType SubType { get; private set; }
        
        /// <summary>
        /// Birth 타입인 경우에만 부모 위치를 지속 추적
        /// </summary>
        public bool TrackParentPosition => IsSubEmitter && SubType == SubEmitterType.Birth;
        
        // 부모로부터 상속받은 기준점/속성
        public Vector2 BasePosition { get; set; }
        public Vector2 InheritedVelocity { get; private set; }
        public Color InheritedColor { get; private set; }
        /// <summary>
        /// 상속받은 크기 비율 (부모 기본 크기 대비 현재 파티클 크기 비율)
        /// </summary>
        public float InheritedSizeRatio { get; private set; }
        public float InheritedRotation { get; private set; }
        
        // 상속 설정
        public SubEmitterInherit InheritFlags { get; private set; }
        
        /// <summary>
        /// Sub Emitter 생성자
        /// </summary>
        /// <param name="id">세션 ID</param>
        /// <param name="subType">Sub Emitter 타입 (Birth/Death)</param>
        /// <param name="parentSystem">부모 파티클 시스템</param>
        /// <param name="parentIndex">부모 파티클 인덱스</param>
        /// <param name="parentParticleId">부모 파티클 고유 ID</param>
        /// <param name="position">기준 위치</param>
        /// <param name="velocity">상속받은 속도</param>
        /// <param name="color">상속받은 색상</param>
        /// <param name="sizeRatio">상속받은 크기 비율</param>
        /// <param name="rotation">상속받은 회전</param>
        /// <param name="inheritFlags">상속 플래그</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="loop">루프 여부</param>
        public ParticleEmission(
            int id, 
            SubEmitterType subType,
            CanvasParticleSystem parentSystem, 
            int parentIndex,
            uint parentParticleId,
            Vector2 position, 
            Vector2 velocity, 
            Color color,
            float sizeRatio,
            float rotation,
            SubEmitterInherit inheritFlags,
            float duration, 
            bool loop)
        {
            Id = id;
            SourceType = EmissionSourceType.SubEmitter;
            SubType = subType;
            ParentSystem = parentSystem;
            // Death 타입은 부모 추적 안함
            ParentParticleIndex = subType == SubEmitterType.Birth ? parentIndex : -1;
            ParentParticleId = parentParticleId;
            
            // 부모로부터 상속받은 정보
            BasePosition = position;
            InheritedVelocity = velocity;
            InheritedSizeRatio = sizeRatio;
            InheritedColor = color;
            InheritedRotation = rotation;
            InheritFlags = inheritFlags;
            
            // 세션 초기화
            Duration = duration;
            IsLooping = loop;
            PlayTime = 0f;
            BurstStates = new List<BurstState>();
            EmissionAccumulator = 0f;
        }
    }
}
