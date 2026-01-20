# Waker Canvas Particle Systems

High-performance particle system for Unity Canvas with mesh batching rendering.

## Features

- **Mesh Batching** - Render thousands of particles in a single draw call
- **Canvas Integration** - Seamless UI integration with RectTransform and masking support
- **Modular Design** - Similar to Unity's ParticleSystem module structure
  - Main Module (duration, loop, lifetime, size, speed, rotation, color)
  - Emission Module (rate over time, bursts)
  - Shape Module (Circle, Cone, Edge, Rectangle with spread modes)
  - Velocity Over Lifetime
  - Size Over Lifetime
  - Color Over Lifetime
  - Noise Module (Perlin noise-based movement)
  - Attraction Module (target-based particle attraction)
  - Texture Sheet Animation
  - Sub Emitter Module (Birth/Death triggers with property inheritance)
- **Sprite Support** - Multiple sprites with automatic renderer pooling
- **Editor Preview** - Real-time particle preview in Scene view

## Installation

### Unity Package Manager (Git URL)

```
https://github.com/wakeup5/Waker.CanvasParticleSystem.git
```

### Requirements

- Unity 2020.3+
- Unity Mathematics 1.2.1+

## Quick Start

1. Create a GameObject under Canvas
2. Add `CanvasParticleSystem` component
3. Configure modules in Inspector
4. Play!

### Basic API

```csharp
using Waker.CanvasParticleSystems;

var ps = GetComponent<CanvasParticleSystem>();

// Playback control
ps.Play();
ps.Pause();
ps.Resume();
ps.Stop();

// Manual emission
ps.EmitSingle();
ps.EmitBurst(10);
ps.Emit(position, velocity, color, size, lifetime);

// Sprite emission
ps.Emit(sprite);
ps.Emit(sprite, count);
```

## License

[MIT License](LICENSE)

## Author

wakeup5

---

*This project was created with AI assistance.*
