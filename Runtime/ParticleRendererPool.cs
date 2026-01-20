using System.Collections.Generic;
using UnityEngine;

namespace Waker.CanvasParticleSystems
{
    /// <summary>
    /// CanvasParticleRenderer 풀 관리 클래스
    /// 스프라이트별 렌더러를 관리하여 재사용
    /// </summary>
    internal class ParticleRendererPool
    {
        private readonly CanvasParticleSystem _owner;
        private readonly Queue<CanvasParticleRenderer> _availableRenderers;
        private readonly Dictionary<Sprite, CanvasParticleRenderer> _activeRenderers;
        private readonly int _maxPoolSize;
        private int _nextSpriteIndex;

        public ParticleRendererPool(CanvasParticleSystem owner, int maxSize)
        {
            _owner = owner;
            _maxPoolSize = maxSize;
            _availableRenderers = new Queue<CanvasParticleRenderer>();
            _activeRenderers = new Dictionary<Sprite, CanvasParticleRenderer>();
            _nextSpriteIndex = 0;
        }

        /// <summary>
        /// 활성화된 렌더러 수
        /// </summary>
        public int ActiveCount => _activeRenderers.Count;

        /// <summary>
        /// 사용 가능한 풀 렌더러 수
        /// </summary>
        public int AvailableCount => _availableRenderers.Count;

        /// <summary>
        /// 스프라이트에 대한 렌더러 가져오기 (풀에서 재사용 또는 새로 생성)
        /// </summary>
        public CanvasParticleRenderer GetRenderer(Sprite sprite)
        {
            // 이미 활성화된 렌더러가 있으면 반환
            if (_activeRenderers.TryGetValue(sprite, out var existing))
                return existing;

            // 풀에서 가져오기 또는 새로 생성
            CanvasParticleRenderer renderer;
            if (_availableRenderers.Count > 0)
            {
                renderer = _availableRenderers.Dequeue();
            }
            else
            {
                renderer = CreateNewRenderer();
            }

            renderer.SetSprite(sprite);
            renderer.SpriteIndex = _nextSpriteIndex++;
            renderer.gameObject.SetActive(true);
            _activeRenderers[sprite] = renderer;
            return renderer;
        }

        /// <summary>
        /// 스프라이트에 해당하는 렌더러 찾기 (없으면 null)
        /// </summary>
        public CanvasParticleRenderer? FindRenderer(Sprite sprite)
        {
            _activeRenderers.TryGetValue(sprite, out var renderer);
            return renderer;
        }

        /// <summary>
        /// 스프라이트 인덱스로 렌더러 찾기
        /// </summary>
        public CanvasParticleRenderer? FindRendererByIndex(int spriteIndex)
        {
            foreach (var renderer in _activeRenderers.Values)
            {
                if (renderer.SpriteIndex == spriteIndex)
                    return renderer;
            }
            return null;
        }

        /// <summary>
        /// 렌더러를 풀로 반환
        /// </summary>
        public void ReturnRenderer(CanvasParticleRenderer renderer)
        {
            if (renderer == null) return;

            var sprite = renderer.CurrentSprite;
            if (sprite != null && _activeRenderers.ContainsKey(sprite))
            {
                _activeRenderers.Remove(sprite);
            }

            renderer.Reset();
            renderer.gameObject.SetActive(false);

            // 풀 크기 제한
            if (_availableRenderers.Count < _maxPoolSize)
            {
                _availableRenderers.Enqueue(renderer);
            }
            else
            {
                Object.Destroy(renderer.gameObject);
            }
        }

        /// <summary>
        /// 빈 렌더러 정리 (파티클이 없는 렌더러를 풀로 반환)
        /// </summary>
        public void CleanupEmptyRenderers()
        {
            var toReturn = new List<CanvasParticleRenderer>();
            
            foreach (var kvp in _activeRenderers)
            {
                if (kvp.Value.ParticleCount == 0)
                {
                    toReturn.Add(kvp.Value);
                }
            }

            foreach (var renderer in toReturn)
            {
                ReturnRenderer(renderer);
            }
        }

        /// <summary>
        /// 모든 활성 렌더러 정리 (풀로 반환)
        /// </summary>
        public void Clear()
        {
            var renderers = new List<CanvasParticleRenderer>(_activeRenderers.Values);
            foreach (var renderer in renderers)
            {
                renderer.Reset();
                renderer.gameObject.SetActive(false);
                
                if (_availableRenderers.Count < _maxPoolSize)
                {
                    _availableRenderers.Enqueue(renderer);
                }
                else
                {
                    Object.Destroy(renderer.gameObject);
                }
            }
            _activeRenderers.Clear();
        }

        /// <summary>
        /// 모든 활성 렌더러 Dirty 표시
        /// </summary>
        public void MarkAllDirty()
        {
            foreach (var renderer in _activeRenderers.Values)
            {
                renderer.MarkDirty();
            }
        }

        /// <summary>
        /// 풀 완전 파괴
        /// </summary>
        public void Destroy()
        {
            // 활성 렌더러 제거
            foreach (var renderer in _activeRenderers.Values)
            {
                if (renderer != null)
                    Object.Destroy(renderer.gameObject);
            }

            // 풀의 렌더러 제거
            while (_availableRenderers.Count > 0)
            {
                var renderer = _availableRenderers.Dequeue();
                if (renderer != null)
                    Object.Destroy(renderer.gameObject);
            }

            _activeRenderers.Clear();
        }

        private CanvasParticleRenderer CreateNewRenderer()
        {
            var go = new GameObject("ParticleRenderer");
            go.transform.SetParent(_owner.transform, false);
            go.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;

            // RectTransform 설정
            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var renderer = go.AddComponent<CanvasParticleRenderer>();
            renderer.Initialize(_owner);
            renderer.raycastTarget = false;

            return renderer;
        }
    }
}
