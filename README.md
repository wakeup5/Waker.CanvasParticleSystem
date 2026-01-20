# Waker Canvas Particle Systems

> `com.waker.canvasparticlesystems` - v0.1.0

Unity Canvas 기반 고성능 파티클 시스템입니다. Mesh 배치 렌더링과 Job System을 활용하여 수천 개의 파티클을 효율적으로 처리합니다.

## 주요 기능

- **Mesh 배치 렌더링** - 하나의 드로우콜로 모든 파티클 렌더링
- **Job System 최적화** - Burst 컴파일러로 멀티스레드 업데이트
- **Canvas 통합** - RectTransform 기반으로 UI와 완벽 통합
- **다양한 방출 패턴** - Point, Circle, Cone, Hemisphere
- **물리 시뮬레이션** - 속도, 중력, 회전 지원
- **색상 그라디언트** - 수명에 따른 색상 변화

## 성능 특징

### Mesh 배치 렌더링
개별 GameObject를 사용하지 않고 하나의 Mesh에 모든 파티클을 그립니다:
- ✅ **1 드로우콜**로 수천 개 파티클 렌더링
- ✅ Transform 업데이트 최소화
- ✅ 메모리 효율적

### Job System + Burst
파티클 업데이트를 멀티스레드로 병렬 처리:
- ✅ CPU 코어 활용 극대화
- ✅ Burst 컴파일러로 네이티브 성능
- ✅ 메인 스레드 부하 최소화

## 설치

### Unity Package Manager (Git URL)

```
https://git.renner.kr/renner/Waker.Libraries.git?path=/Waker.CanvasParticleSystems
```

### 의존성

- Unity 2020.3 이상
- .NET Standard 2.1
- Unity Burst 1.6.0+
- Unity Jobs 0.11.0+
- Unity Mathematics 1.2.1+

## 빠른 시작

### 1. CanvasParticleSystem 추가

Canvas 하위에 GameObject를 생성하고 `CanvasParticleSystem` 컴포넌트를 추가합니다.

```csharp
using UnityEngine;
using Waker.CanvasParticleSystems;

public class ParticleExample : MonoBehaviour
{
    void Start()
    {
        var particleSystem = gameObject.AddComponent<CanvasParticleSystem>();
        particleSystem.MaxParticles = 1000;
        particleSystem.Gravity = -100f;
    }
}
```

### 2. 파티클 방출

```csharp
// 직접 방출
Vector2 position = Vector2.zero;
Vector2 velocity = new Vector2(0, 100);
Color color = Color.white;
float size = 20f;
float lifetime = 2f;

particleSystem.Emit(position, velocity, color, size, lifetime);
```

### 3. Emitter 사용 (권장)

#### 3-1. EmissionSettings 에셋 생성

Project 창에서:
1. 우클릭 → `Create > Waker > Canvas Particle Systems > Emission Settings`
2. Inspector에서 방출 설정 구성

#### 3-2. ParticleEmitter 추가

```csharp
var emitter = gameObject.AddComponent<ParticleEmitter>();
emitter.EmissionSettings = emissionSettings; // ScriptableObject
emitter.Play();
```

## 사용 예제

### 버스트 효과

```csharp
public class BurstEffect : MonoBehaviour
{
    [SerializeField] private ParticleEmitter emitter;
    
    public void Explode()
    {
        emitter.EmitBurst(100); // 100개 파티클 즉시 방출
    }
}
```

### 특정 위치에서 방출

```csharp
public class ClickParticle : MonoBehaviour
{
    [SerializeField] private ParticleEmitter emitter;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            emitter.EmitAt(worldPos, 50);
        }
    }
}
```

### 연속 방출

```csharp
// Inspector에서 설정
Auto Emit: true
Emission Rate: 0.1 (초당 10개)
Particles Per Emission: 1

// 또는 코드로
emitter.AutoEmit = true;
emitter.EmissionRate = 0.1f;
emitter.ParticlesPerEmission = 1;
emitter.Play();
```

## EmissionSettings 옵션

### 방출 패턴
- **Point** - 한 점에서 방출
- **Circle** - 원형으로 360도 방출
- **Cone** - 지정된 각도로 원뿔형 방출
- **Hemisphere** - 반구형 180도 방출

### 파티클 속성
- **Speed Range** - 초기 속도 범위
- **Size Range** - 크기 범위
- **Lifetime Range** - 수명 범위
- **Rotation Speed Range** - 회전 속도 범위

### 색상
- **Start Color** - 시작 색상
- **End Color** - 종료 색상
- **Use Gradient** - 그라디언트 사용 여부
- **Color Gradient** - 수명에 따른 색상 곡선

## API 레퍼런스

### CanvasParticleSystem

#### Properties
- `int MaxParticles` - 최대 파티클 개수
- `int ActiveParticleCount` - 현재 활성 파티클 수
- `float Gravity` - 중력 가속도

#### Methods
- `void Emit(Vector2 position, Vector2 velocity, Color color, float size, float lifetime, float rotation = 0, float rotationSpeed = 0)` - 파티클 방출
- `void Clear()` - 모든 파티클 제거

### ParticleEmitter

#### Methods
- `void Play()` - 자동 방출 시작
- `void Stop()` - 자동 방출 중지
- `void EmitBurst(int count)` - 즉시 방출
- `void EmitSingle()` - 파티클 하나 방출
- `void EmitAt(Vector2 worldPosition, int count = 1)` - 특정 위치에서 방출
- `void Clear()` - 모든 파티클 제거

## 성능 팁

1. **Job System 활성화** - Inspector에서 `Use Job System` 체크 (기본값)
2. **적절한 최대 개수** - MaxParticles를 필요한 만큼만 설정
3. **수명 관리** - 짧은 수명으로 파티클 재사용 최적화
4. **텍스처 아틀라스** - 여러 효과를 하나의 텍스처로 통합

## 제한 사항

- 모든 파티클이 동일한 Material/Texture 사용
- Canvas Overlay/Camera 모드에서만 작동
- World Space Canvas는 지원하지만 성능 고려 필요

## 라이선스

MIT License

## Author

renner
