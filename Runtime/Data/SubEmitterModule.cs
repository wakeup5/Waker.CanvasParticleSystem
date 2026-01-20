using System;
using System.Collections.Generic;
using UnityEngine;

namespace Waker.CanvasParticleSystems
{
    /// <summary>
    /// Sub Emitter 모듈 - 부모 파티클의 생명주기 이벤트에 반응하여 
    /// 자식 파티클 시스템에 ParticleEmission을 생성합니다.
    /// </summary>
    [Serializable]
    public class SubEmitterModule
    {
        [Tooltip("서브 이미터 활성화")]
        public bool enabled = false;
        
        [Tooltip("서브 이미터 목록")]
        public List<SubEmitterEntry> subEmitters = new List<SubEmitterEntry>();
    }

    /// <summary>
    /// 개별 서브 이미터 설정
    /// </summary>
    [Serializable]
    public class SubEmitterEntry
    {
        [Tooltip("방출 시점\n- Birth: 부모 파티클 생성 시 (위치 지속 추적)\n- Death: 부모 파티클 소멸 시 (마지막 위치)")]
        public SubEmitterType type = SubEmitterType.Birth;
        
        [Tooltip("자식 파티클 시스템 - 자식 시스템의 모든 모듈 설정을 그대로 사용")]
        public CanvasParticleSystem? subParticleSystem;
        
        [Tooltip("부모로부터 상속받을 속성")]
        public SubEmitterInherit inherit = SubEmitterInherit.Position;
        
        [Tooltip("부모 속도 상속 비율 (Velocity 플래그 활성 시)")]
        [Range(0f, 1f)]
        public float inheritVelocityMultiplier = 0.5f;
        
        [Tooltip("부모 크기 상속 비율 (Size 플래그 활성 시)")]
        [Range(0f, 2f)]
        public float inheritSizeMultiplier = 1.0f;
    }
}
