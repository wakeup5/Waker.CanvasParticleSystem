using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Waker.CanvasParticleSystems
{
    /// <summary>
    /// 단일 스프라이트를 렌더링하는 서브 렌더러
    /// CanvasParticleSystem의 자식으로 생성되어 특정 스프라이트의 파티클만 렌더링
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class CanvasParticleRenderer : MaskableGraphic
    {
        private CanvasParticleSystem? _parentSystem;
        private Sprite? _sprite;
        private readonly List<int> _particleIndices = new List<int>();
        
        /// <summary>
        /// 현재 렌더링 중인 스프라이트
        /// </summary>
        public Sprite? CurrentSprite => _sprite;
        
        /// <summary>
        /// 이 렌더러가 담당하는 파티클 개수
        /// </summary>
        public int ParticleCount => _particleIndices.Count;
        
        /// <summary>
        /// 렌더러에 할당된 스프라이트 인덱스 (풀 관리용)
        /// </summary>
        internal int SpriteIndex { get; set; } = -1;
        
        public override Texture mainTexture => _sprite != null ? _sprite.texture : Texture2D.whiteTexture;

        /// <summary>
        /// 부모 파티클 시스템 초기화
        /// </summary>
        internal void Initialize(CanvasParticleSystem parent)
        {
            _parentSystem = parent;
            raycastTarget = false;
        }

        /// <summary>
        /// 스프라이트 설정
        /// </summary>
        internal void SetSprite(Sprite? sprite)
        {
            _sprite = sprite;
            SetMaterialDirty();
        }

        /// <summary>
        /// 파티클 인덱스 추가
        /// </summary>
        internal void AddParticle(int index)
        {
            if (!_particleIndices.Contains(index))
            {
                _particleIndices.Add(index);
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// 파티클 인덱스 제거
        /// </summary>
        internal void RemoveParticle(int index)
        {
            if (_particleIndices.Remove(index))
            {
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// 렌더러 초기화 (풀 반환 시 호출)
        /// </summary>
        internal new void Reset()
        {
            _particleIndices.Clear();
            _sprite = null;
            SpriteIndex = -1;
            SetVerticesDirty();
        }

        /// <summary>
        /// Dirty 상태 갱신 요청
        /// </summary>
        internal void MarkDirty()
        {
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (_parentSystem == null || _sprite == null || _particleIndices.Count == 0)
                return;

            // 스프라이트 UV 계산
            var uvRect = GetSpriteUVRect(_sprite);

            foreach (var index in _particleIndices)
            {
                if (!_parentSystem.IsParticleAlive(index))
                    continue;

                var particle = _parentSystem.GetParticleData(index);
                AddParticleQuad(vh, particle, uvRect);
            }
        }

        private void AddParticleQuad(VertexHelper vh, ParticleData particle, Vector4 uvRect)
        {
            int vertexIndex = vh.currentVertCount;

            // 회전 및 스케일 적용
            float halfSize = particle.Size * 0.5f;
            float cos = math.cos(particle.Rotation);
            float sin = math.sin(particle.Rotation);

            // 4개의 정점 위치 계산
            Vector2 v0 = RotatePoint(new float2(-halfSize, -halfSize), cos, sin);
            Vector2 v1 = RotatePoint(new float2(-halfSize, halfSize), cos, sin);
            Vector2 v2 = RotatePoint(new float2(halfSize, halfSize), cos, sin);
            Vector2 v3 = RotatePoint(new float2(halfSize, -halfSize), cos, sin);

            // 파티클 위치 적용
            Vector3 pos = new Vector3(particle.Position.x, particle.Position.y, 0);
            Color32 color = new Color(particle.Color.x, particle.Color.y, particle.Color.z, particle.Color.w);

            // UV 좌표 (스프라이트 rect 적용)
            Vector2 uv0 = new Vector2(uvRect.x, uvRect.y);
            Vector2 uv1 = new Vector2(uvRect.x, uvRect.y + uvRect.w);
            Vector2 uv2 = new Vector2(uvRect.x + uvRect.z, uvRect.y + uvRect.w);
            Vector2 uv3 = new Vector2(uvRect.x + uvRect.z, uvRect.y);

            // 정점 추가
            vh.AddVert(pos + (Vector3)v0, color, uv0);
            vh.AddVert(pos + (Vector3)v1, color, uv1);
            vh.AddVert(pos + (Vector3)v2, color, uv2);
            vh.AddVert(pos + (Vector3)v3, color, uv3);

            // 인덱스 추가 (2개의 삼각형)
            vh.AddTriangle(vertexIndex + 0, vertexIndex + 1, vertexIndex + 2);
            vh.AddTriangle(vertexIndex + 2, vertexIndex + 3, vertexIndex + 0);
        }

        private Vector2 RotatePoint(float2 point, float cos, float sin)
        {
            return new Vector2(
                point.x * cos - point.y * sin,
                point.x * sin + point.y * cos
            );
        }

        /// <summary>
        /// 스프라이트의 UV 좌표 계산 (x, y, width, height)
        /// </summary>
        private Vector4 GetSpriteUVRect(Sprite sprite)
        {
            if (sprite.texture == null)
                return new Vector4(0, 0, 1, 1);

            var rect = sprite.textureRect;
            float texWidth = sprite.texture.width;
            float texHeight = sprite.texture.height;

            return new Vector4(
                rect.x / texWidth,
                rect.y / texHeight,
                rect.width / texWidth,
                rect.height / texHeight
            );
        }
    }
}
