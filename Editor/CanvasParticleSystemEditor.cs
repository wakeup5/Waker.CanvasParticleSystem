using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;

namespace Waker.CanvasParticleSystems.Editor
{
    [CustomEditor(typeof(CanvasParticleSystem))]
    public class CanvasParticleSystemEditor : UnityEditor.Editor
    {
        private SerializedProperty _mainModule = null!;
        private SerializedProperty _emissionModule = null!;
        private SerializedProperty _shapeModule = null!;
        private SerializedProperty _velocityOverLifetime = null!;
        private SerializedProperty _noiseModule = null!;
        private SerializedProperty _sizeOverLifetime = null!;
        private SerializedProperty _colorOverLifetime = null!;
        private SerializedProperty _attractionModule = null!;
        private SerializedProperty _rendererModule = null!;
        private SerializedProperty _textureSheetAnimation = null!;
        private SerializedProperty _subEmitterModule = null!;

        private bool _showMain = true;
        private bool _showTexture = true;
        private bool _showTextureSheet = false;
        private bool _showEmission = true;
        private bool _showShape = true;
        private bool _showVelocity = true;
        private bool _showNoise = true;
        private bool _showSizeOverLifetime = true;
        private bool _showColorOverLifetime = true;
        private bool _showAttraction = true;
        private bool _showSubEmitter = true;

        private CanvasParticleSystem _particle = null!;
        private ReorderableList? _burstList;
        private ReorderableList? _subEmitterList;

        private void OnEnable()
        {
            _particle = (CanvasParticleSystem)target;

            _mainModule = serializedObject.FindProperty("_mainModule");
            _emissionModule = serializedObject.FindProperty("_emissionModule");
            _shapeModule = serializedObject.FindProperty("_shapeModule");
            _velocityOverLifetime = serializedObject.FindProperty("_velocityOverLifetime");
            _noiseModule = serializedObject.FindProperty("_noiseModule");
            _sizeOverLifetime = serializedObject.FindProperty("_sizeOverLifetime");
            _colorOverLifetime = serializedObject.FindProperty("_colorOverLifetime");
            _attractionModule = serializedObject.FindProperty("_attractionModule");
            _rendererModule = serializedObject.FindProperty("_rendererModule");
            _textureSheetAnimation = serializedObject.FindProperty("_textureSheetAnimation");
            _subEmitterModule = serializedObject.FindProperty("_subEmitterModule");

            SetupBurstList();
            SetupSubEmitterList();

            EditorApplication.update += OnEditorUpdate;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnEditorUpdate()
        {
            if (_particle != null && Selection.activeGameObject == _particle.gameObject)
            {
                // 재생 중일 때만 SceneView 리페인트 (Inspector는 업데이트 안함)
                if (_particle.IsPlaying && !_particle.IsPaused)
                {
                    SceneView.RepaintAll();
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (_particle == null) return;

            // Shape 기즈모 그리기 및 편집
            if (_particle.Shape.enabled)
            {
                Transform pivot = _particle.Shape.pivot != null ? _particle.Shape.pivot : _particle.transform;
                Matrix4x4 handleMatrix = pivot.localToWorldMatrix;

                // 기준 Transform 위치 핸들 (pivot이 없을 때만)
                if (_particle.Shape.pivot != null)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPivotPos = Handles.PositionHandle(_particle.Shape.pivot.position, _particle.Shape.pivot.rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_particle.Shape.pivot, "Move Shape Pivot");
                        _particle.Shape.pivot.position = newPivotPos;
                    }
                }

                Handles.color = new Color(0.3f, 0.8f, 1f, 0.5f);

                using (new Handles.DrawingScope(handleMatrix))
                {
                    float emissionAngleRad = _particle.Shape.emissionAngle * Mathf.Deg2Rad;

                    switch (_particle.Shape.shape)
                    {
                        case EmissionShape.Circle:
                            // 원 그리기
                            Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _particle.Shape.radius);
                            Handles.DrawSolidDisc(Vector3.zero, Vector3.forward, _particle.Shape.radius * 0.05f);

                            // 반지름 핸들
                            EditorGUI.BeginChangeCheck();
                            Vector3 radiusHandlePos = Vector3.right * _particle.Shape.radius;
                            float handleSize = HandleUtility.GetHandleSize(pivot.TransformPoint(radiusHandlePos)) * 0.08f;
                            Vector3 newRadiusPos = Handles.Slider(radiusHandlePos, Vector3.right, handleSize, Handles.SphereHandleCap, 0.1f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_particle, "Change Circle Radius");
                                _particle.Shape.radius = Mathf.Max(0, newRadiusPos.x);
                                EditorUtility.SetDirty(_particle);
                            }
                            break;

                        case EmissionShape.Cone:
                            float halfAngle = _particle.Shape.angle * 0.5f;
                            Vector3 centerDir = new Vector3(Mathf.Cos(emissionAngleRad), Mathf.Sin(emissionAngleRad), 0);
                            float leftAngle = emissionAngleRad - halfAngle * Mathf.Deg2Rad;
                            Vector3 leftDir = new Vector3(Mathf.Cos(leftAngle), Mathf.Sin(leftAngle), 0);
                            float rightAngle = emissionAngleRad + halfAngle * Mathf.Deg2Rad;
                            Vector3 rightDir = new Vector3(Mathf.Cos(rightAngle), Mathf.Sin(rightAngle), 0);

                            // 원뿔 호 그리기
                            Handles.DrawWireArc(Vector3.zero, Vector3.forward, leftDir, _particle.Shape.angle, _particle.Shape.radius);
                            Handles.DrawLine(Vector3.zero, leftDir * _particle.Shape.radius);
                            Handles.DrawLine(Vector3.zero, rightDir * _particle.Shape.radius);
                            Handles.DrawSolidDisc(Vector3.zero, Vector3.forward, _particle.Shape.radius * 0.05f);

                            // 반지름 핸들
                            EditorGUI.BeginChangeCheck();
                            Vector3 coneRadiusPos = centerDir * _particle.Shape.radius;
                            float coneHandleSize = HandleUtility.GetHandleSize(pivot.TransformPoint(coneRadiusPos)) * 0.08f;
                            Vector3 newConeRadiusPos = Handles.Slider(coneRadiusPos, centerDir, coneHandleSize, Handles.SphereHandleCap, 0.1f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_particle, "Change Cone Radius");
                                _particle.Shape.radius = Mathf.Max(0, newConeRadiusPos.magnitude);
                                EditorUtility.SetDirty(_particle);
                            }

                            // 각도 핸들 (왼쪽)
                            Handles.color = Color.yellow;
                            EditorGUI.BeginChangeCheck();
                            Vector3 leftHandlePos = leftDir * _particle.Shape.radius;
                            Vector3 newLeftPos = Handles.Slider(leftHandlePos, new Vector3(-leftDir.y, leftDir.x, 0), coneHandleSize, Handles.SphereHandleCap, 0.1f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_particle, "Change Cone Angle");
                                float newAngle = Mathf.Atan2(newLeftPos.y, newLeftPos.x) * Mathf.Rad2Deg;
                                float angleDiff = _particle.Shape.emissionAngle - newAngle;
                                _particle.Shape.angle = Mathf.Clamp(angleDiff * 2f, 0f, 360f);
                                EditorUtility.SetDirty(_particle);
                            }
                            break;

                        case EmissionShape.Edge:
                            Vector3 edgeDir = new Vector3(Mathf.Cos(emissionAngleRad), Mathf.Sin(emissionAngleRad), 0);
                            Vector3 edgePerp = new Vector3(-Mathf.Sin(emissionAngleRad), Mathf.Cos(emissionAngleRad), 0);
                            Vector3 edgeStart = edgePerp * (-_particle.Shape.size.x * 0.5f);
                            Vector3 edgeEnd = edgePerp * (_particle.Shape.size.x * 0.5f);

                            Handles.DrawLine(edgeStart, edgeEnd);
                            Handles.DrawSolidDisc(edgeStart, Vector3.forward, Mathf.Max(_particle.Shape.size.x * 0.02f, 2f));
                            Handles.DrawSolidDisc(edgeEnd, Vector3.forward, Mathf.Max(_particle.Shape.size.x * 0.02f, 2f));

                            // 방향 화살표
                            Handles.color = Color.yellow;
                            Vector3 arrowEnd = edgeDir * 20f;
                            Handles.DrawLine(Vector3.zero, arrowEnd);
                            Handles.ConeHandleCap(0, arrowEnd, Quaternion.LookRotation(Vector3.forward, edgeDir), 5f, EventType.Repaint);

                            // 길이 핸들
                            Handles.color = new Color(0.3f, 0.8f, 1f, 0.8f);
                            EditorGUI.BeginChangeCheck();
                            float edgeHandleSize = HandleUtility.GetHandleSize(pivot.TransformPoint(edgeEnd)) * 0.08f;
                            Vector3 newEdgeEnd = Handles.Slider(edgeEnd, edgePerp, edgeHandleSize, Handles.SphereHandleCap, 0.1f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_particle, "Change Edge Size");
                                _particle.Shape.size = new Vector2(Mathf.Max(0, Vector3.Dot(newEdgeEnd, edgePerp) * 2f), _particle.Shape.size.y);
                                EditorUtility.SetDirty(_particle);
                            }
                            break;

                        case EmissionShape.Rectangle:
                            float rectAngle = emissionAngleRad;
                            Vector3 rectDir = new Vector3(Mathf.Cos(rectAngle), Mathf.Sin(rectAngle), 0);
                            Vector3 rectRight = new Vector3(-Mathf.Sin(rectAngle), Mathf.Cos(rectAngle), 0);
                            Vector3 halfSize = new Vector3(_particle.Shape.size.x * 0.5f, _particle.Shape.size.y * 0.5f, 0);

                            Vector3[] rectCorners = new Vector3[4];
                            rectCorners[0] = -rectRight * halfSize.x - rectDir * halfSize.y;
                            rectCorners[1] = rectRight * halfSize.x - rectDir * halfSize.y;
                            rectCorners[2] = rectRight * halfSize.x + rectDir * halfSize.y;
                            rectCorners[3] = -rectRight * halfSize.x + rectDir * halfSize.y;
                            Handles.DrawSolidRectangleWithOutline(rectCorners, new Color(0.3f, 0.8f, 1f, 0.1f), new Color(0.3f, 0.8f, 1f, 0.8f));

                            // 방향 화살표
                            Handles.color = Color.yellow;
                            Vector3 rectArrowEnd = rectDir * 20f;
                            Handles.DrawLine(Vector3.zero, rectArrowEnd);
                            Handles.ConeHandleCap(0, rectArrowEnd, Quaternion.LookRotation(Vector3.forward, rectDir), 5f, EventType.Repaint);

                            // 가로 크기 핸들
                            Handles.color = new Color(1f, 0.3f, 0.3f, 0.8f);
                            EditorGUI.BeginChangeCheck();
                            Vector3 rightHandle = rectRight * halfSize.x;
                            float rectHandleSize = HandleUtility.GetHandleSize(pivot.TransformPoint(rightHandle)) * 0.08f;
                            Vector3 newRightHandle = Handles.Slider(rightHandle, rectRight, rectHandleSize, Handles.SphereHandleCap, 0.1f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_particle, "Change Rectangle Width");
                                _particle.Shape.size = new Vector2(Mathf.Max(0, Vector3.Dot(newRightHandle, rectRight) * 2f), _particle.Shape.size.y);
                                EditorUtility.SetDirty(_particle);
                            }

                            // 세로 크기 핸들
                            Handles.color = new Color(0.3f, 1f, 0.3f, 0.8f);
                            EditorGUI.BeginChangeCheck();
                            Vector3 topHandle = rectDir * halfSize.y;
                            Vector3 newTopHandle = Handles.Slider(topHandle, rectDir, rectHandleSize, Handles.SphereHandleCap, 0.1f);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(_particle, "Change Rectangle Height");
                                _particle.Shape.size = new Vector2(_particle.Shape.size.x, Mathf.Max(0, Vector3.Dot(newTopHandle, rectDir) * 2f));
                                EditorUtility.SetDirty(_particle);
                            }
                            break;
                    }

                    // Emission Angle 핸들 (모든 Shape 공통)
                    Handles.color = new Color(1f, 0.8f, 0.2f, 0.8f);
                    Vector3 angleDir = new Vector3(Mathf.Cos(emissionAngleRad), Mathf.Sin(emissionAngleRad), 0);
                    float angleHandleDist = 40f;
                    Vector3 angleHandlePos = angleDir * angleHandleDist;
                    float angleHandleSize = HandleUtility.GetHandleSize(pivot.TransformPoint(angleHandlePos)) * 0.06f;

                    EditorGUI.BeginChangeCheck();
                    Vector3 newAnglePos = Handles.Slider2D(
                        angleHandlePos,
                        Vector3.forward,
                        Vector3.right,
                        Vector3.up,
                        angleHandleSize,
                        Handles.CircleHandleCap,
                        0.1f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_particle, "Change Emission Angle");
                        _particle.Shape.emissionAngle = Mathf.Atan2(newAnglePos.y, newAnglePos.x) * Mathf.Rad2Deg;
                        EditorUtility.SetDirty(_particle);
                    }

                    // 각도 호 표시
                    Handles.color = new Color(1f, 0.8f, 0.2f, 0.3f);
                    Handles.DrawSolidArc(Vector3.zero, Vector3.forward, Vector3.right, _particle.Shape.emissionAngle, angleHandleDist * 0.8f);
                }
            }

            // 씬뷰 패널 (우측 하단) - 재생 컨트롤만
            Handles.BeginGUI();

            // Sub Emitter 자식 시스템 체크 및 부모 시스템 찾기
            var parentSystem = GetParentSubEmitterSystem(_particle);
            CanvasParticleSystem controlTarget = parentSystem ?? _particle;
            
            float panelWidth = 240f;
            float panelHeight = parentSystem != null ? 70f : 50f; // Sub Emitter 표시 시 높이 증가
            float margin = 10f;
            
            // SceneView 영역 크기 (카메라 뷰포트)
            Rect cameraRect = sceneView.camera.pixelRect;
            
            Rect panelRect = new Rect(
                cameraRect.width - panelWidth - margin,
                cameraRect.height - panelHeight - margin,
                panelWidth,
                panelHeight
            );

            // Unity Particle System 스타일 배경
            GUIStyle bgStyle = new GUIStyle(GUI.skin.box);
            bgStyle.normal.background = MakeTexture(2, 2, new Color(0.15f, 0.15f, 0.15f, 0.95f));
            bgStyle.border = new RectOffset(3, 3, 3, 3);
            GUI.Box(panelRect, GUIContent.none, bgStyle);

            GUILayout.BeginArea(new Rect(panelRect.x + 8, panelRect.y + 8, panelRect.width - 16, panelRect.height - 16));
            
            if (parentSystem != null)
            {
                // Sub Emitter 자식은 부모 시스템을 제어
                GUIStyle infoStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 9,
                    wordWrap = true
                };
                infoStyle.normal.textColor = new Color(1f, 0.8f, 0.3f);
                GUILayout.Label($"Sub Emitter - 부모: {parentSystem.name}", infoStyle, GUILayout.Height(16));
            }
            
            // 컨트롤 버튼 (controlTarget 사용)
            GUILayout.BeginHorizontal();
                
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 11,
                    padding = new RectOffset(4, 4, 2, 2)
                };

                bool isPlaying = controlTarget.IsPlaying;
                bool isPaused = controlTarget.IsPaused;
                
                // Play 버튼 - Unity 기본 동작: Playing이면 Restart, Paused면 Resume, Stopped면 Play
                GUI.backgroundColor = isPlaying && !isPaused ? new Color(0.3f, 0.8f, 0.3f) : Color.white;
                if (GUILayout.Button(isPlaying && !isPaused ? "▶ Playing" : "▶ Play", buttonStyle, GUILayout.Height(28)))
                {
                    if (isPlaying && !isPaused)
                    {
                        // Playing 상태에서 클릭 = Restart (파티클 제거 후 재시작)
                        controlTarget.Stop(); // clearParticles: true (기본값)
                        controlTarget.Play();
                    }
                    else if (isPaused)
                    {
                        // Paused 상태에서 클릭 = Resume
                        controlTarget.Resume();
                    }
                    else
                    {
                        // Stopped 상태에서 클릭 = Play
                        controlTarget.Play();
                    }
                    EditorUtility.SetDirty(controlTarget);
                }
                
                // Pause 버튼 - Playing/Paused 토글
                GUI.backgroundColor = isPaused ? new Color(1f, 0.8f, 0.3f) : Color.white;
                if (GUILayout.Button(isPaused ? "❚❚ Paused" : "❚❚ Pause", buttonStyle, GUILayout.Height(28)))
                {
                    if (isPaused)
                    {
                        controlTarget.Resume();
                    }
                    else if (isPlaying)
                    {
                        controlTarget.Pause();
                    }
                    EditorUtility.SetDirty(controlTarget);
                    SceneView.RepaintAll();
                    Repaint();
                }
                
                // Stop 버튼 - 정지하고 파티클 제거
                GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
                if (GUILayout.Button("■ Stop", buttonStyle, GUILayout.Height(28)))
                {
                    controlTarget.Stop(); // 기본값 clearParticles: true 사용
                    EditorUtility.SetDirty(controlTarget);
                }
                
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();

            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var particleSystem = (CanvasParticleSystem)target;

            // Main 모듈 (헤더 포함)
            GUILayout.Space(2);
            DrawModuleHeader("Canvas Particle System", ref _showMain, null);
            if (_showMain)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawMainModule(particleSystem), true);
            }
            GUILayout.Space(4);

            // Renderer 모듈
            DrawModuleHeader("Renderer", ref _showTexture, null);
            if (_showTexture)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawRendererModule(), true);
            }
            GUILayout.Space(4);
            
            // Texture Sheet Animation 모듈
            var textureSheetEnabled = _textureSheetAnimation.FindPropertyRelative("enabled");
            DrawModuleHeader("Texture Sheet Animation", ref _showTextureSheet, textureSheetEnabled);
            if (_showTextureSheet)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawTextureSheetAnimationModule(), textureSheetEnabled.boolValue);
            }
            GUILayout.Space(4);

            var emissionEnabled = _emissionModule.FindPropertyRelative("enabled");
            DrawModuleHeader("Emission", ref _showEmission, emissionEnabled);
            if (_showEmission)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawEmissionModule(), emissionEnabled.boolValue);
            }
            GUILayout.Space(4);

            var shapeEnabled = _shapeModule.FindPropertyRelative("enabled");
            DrawModuleHeader("Shape", ref _showShape, shapeEnabled);
            if (_showShape)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawShapeModule(), shapeEnabled.boolValue);
            }
            GUILayout.Space(4);

            var velocityEnabled = _velocityOverLifetime.FindPropertyRelative("enabled");
            DrawModuleHeader("Velocity Over Lifetime", ref _showVelocity, velocityEnabled);
            if (_showVelocity)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawVelocityOverLifetimeModule(), velocityEnabled.boolValue);
            }
            GUILayout.Space(4);

            var noiseEnabled = _noiseModule.FindPropertyRelative("enabled");
            DrawModuleHeader("Noise", ref _showNoise, noiseEnabled);
            if (_showNoise)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawNoiseModule(), noiseEnabled.boolValue);
            }
            GUILayout.Space(4);

            var sizeEnabled = _sizeOverLifetime.FindPropertyRelative("enabled");
            DrawModuleHeader("Size Over Lifetime", ref _showSizeOverLifetime, sizeEnabled);
            if (_showSizeOverLifetime)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawSizeOverLifetimeModule(), sizeEnabled.boolValue);
            }
            GUILayout.Space(4);

            var colorEnabled = _colorOverLifetime.FindPropertyRelative("enabled");
            DrawModuleHeader("Color Over Lifetime", ref _showColorOverLifetime, colorEnabled);
            if (_showColorOverLifetime)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawColorOverLifetimeModule(), colorEnabled.boolValue);
            }
            GUILayout.Space(4);

            var attractionEnabled = _attractionModule.FindPropertyRelative("enabled");
            DrawModuleHeader("Attraction", ref _showAttraction, attractionEnabled);
            if (_showAttraction)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawAttractionModule(), attractionEnabled.boolValue);
            }
            GUILayout.Space(4);
            
            var subEmitterEnabled = _subEmitterModule.FindPropertyRelative("enabled");
            DrawModuleHeader("Sub Emitters", ref _showSubEmitter, subEmitterEnabled);
            if (_showSubEmitter)
            {
                GUILayout.Space(2);
                DrawModuleBg(() => DrawSubEmitterModule(particleSystem), subEmitterEnabled.boolValue);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawModuleHeader(string title, ref bool foldout, SerializedProperty? enabledProperty)
        {
            bool isEnabled = enabledProperty == null || enabledProperty.boolValue;

            GUIContent headerLabel = new GUIContent(title);
            Rect moduleHeaderRect = GUILayoutUtility.GetRect(16f, 15f);

            Rect checkMarkRect = new Rect(moduleHeaderRect.x + 2, moduleHeaderRect.y + 1, 13, 13);
            
            Event e = Event.current;
            
            // Handle checkbox click
            if (enabledProperty != null && e.type == EventType.MouseDown && e.button == 0 && checkMarkRect.Contains(e.mousePosition))
            {
                enabledProperty.boolValue = !enabledProperty.boolValue;
                e.Use();
            }

            // Handle foldout toggle
            if (e.type == EventType.MouseDown && e.button == 0 && moduleHeaderRect.Contains(e.mousePosition) && !checkMarkRect.Contains(e.mousePosition))
            {
                foldout = !foldout;
                e.Use();
            }

            // Draw everything in Repaint (가장 위에 렌더링)
            if (e.type == EventType.Repaint)
            {
                GUIStyle moduleHeaderStyle = (GUIStyle)"ShurikenModuleTitle";
                moduleHeaderStyle.Draw(moduleHeaderRect, false, false, false, false);

                GUIStyle labelStyle = new GUIStyle(moduleHeaderStyle);
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.normal.textColor = isEnabled ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                GUI.Label(moduleHeaderRect, headerLabel, labelStyle);

                if (enabledProperty != null)
                {
                    GUIStyle checkboxStyle = (GUIStyle)"ShurikenCheckMark";
                    checkboxStyle.Draw(checkMarkRect, GUIContent.none, false, false, enabledProperty.boolValue, false);
                }
            }
        }

        private bool DrawHeader(Rect rect, GUIContent label, bool foldout, bool isEnabled, Rect checkMarkRect)
        {
            GUIStyle moduleHeaderStyle = (GUIStyle)"ShurikenModuleTitle";

            // Draw background
            if (Event.current.type == EventType.Repaint)
            {
                moduleHeaderStyle.Draw(rect, false, false, false, false);
            }

            // Handle mouse click for foldout (체크박스 영역 제외)
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition) && !checkMarkRect.Contains(e.mousePosition))
            {
                return !foldout;
            }

            // Draw label
            GUIStyle labelStyle = new GUIStyle(moduleHeaderStyle);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = isEnabled ? Color.white : new Color(0.7f, 0.7f, 0.7f);
            GUI.Label(rect, label, labelStyle);

            return foldout;
        }

        private void DrawModuleBg(System.Action drawAction, bool enabled)
        {
            using (new EditorGUI.DisabledScope(!enabled))
            {
                // EditorGUI.indentLevel++;
                drawAction();
                // EditorGUI.indentLevel--;
            }
        }

        private void DrawMainModule(CanvasParticleSystem particleSystem)
        {
            // Duration & Loop
            var miniLabelStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
            var fieldStyle = (GUIStyle)"ShurikenTextField";
            EditorGUILayout.PropertyField(_mainModule.FindPropertyRelative("duration"), new GUIContent("지속 시간"));
            EditorGUILayout.PropertyField(_mainModule.FindPropertyRelative("loop"), new GUIContent("루프"));
            EditorGUILayout.PropertyField(_mainModule.FindPropertyRelative("playOnAwake"), new GUIContent("시작 시 재생"));
            
            EditorGUILayout.IntField("최대 파티클 수", particleSystem.MaxParticles);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("활성 파티클", particleSystem.ActiveParticleCount);
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.PropertyField(_mainModule.FindPropertyRelative("startLifetime"), new GUIContent("수명"));
            EditorGUILayout.PropertyField(_mainModule.FindPropertyRelative("startSize"), new GUIContent("시작 크기"));
            EditorGUILayout.PropertyField(_mainModule.FindPropertyRelative("startSpeed"), new GUIContent("속도"));
            
            EditorGUILayout.PropertyField(_mainModule.FindPropertyRelative("startRotation"), new GUIContent("시작 회전"));
            EditorGUILayout.PropertyField(_mainModule.FindPropertyRelative("angularVelocity"), new GUIContent("회전 속도"));
            EditorGUILayout.PropertyField(_mainModule.FindPropertyRelative("startColor"), new GUIContent("시작 색상"));
        }

        private void SetupBurstList()
        {
            var burstsProp = _emissionModule.FindPropertyRelative("bursts");
            
            _burstList = new ReorderableList(serializedObject, burstsProp, true, true, true, true);
            
            // 헤더 그리기
            _burstList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Bursts");
            };
            
            // 각 요소 그리기 (한 줄로 컬팩트하게)
            _burstList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = burstsProp.GetArrayElementAtIndex(index);
                rect.y += 2;
                
                float labelWidth = 40;
                float fieldWidth = (rect.width - labelWidth * 4 - 10) / 4;
                float x = rect.x;
                
                // Time
                EditorGUI.LabelField(new Rect(x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight), "Time");
                x += labelWidth;
                EditorGUI.PropertyField(
                    new Rect(x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("time"),
                    GUIContent.none
                );
                x += fieldWidth + 5;
                
                // Count
                EditorGUI.LabelField(new Rect(x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight), "Count");
                x += labelWidth;
                EditorGUI.PropertyField(
                    new Rect(x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("count"),
                    GUIContent.none
                );
                x += fieldWidth + 5;
                
                // Cycles
                EditorGUI.LabelField(new Rect(x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight), "Cycles");
                x += labelWidth;
                EditorGUI.PropertyField(
                    new Rect(x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("cycles"),
                    GUIContent.none
                );
                x += fieldWidth + 5;
                
                // Interval
                EditorGUI.LabelField(new Rect(x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight), "Interval");
                x += labelWidth;
                EditorGUI.PropertyField(
                    new Rect(x, rect.y, fieldWidth, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("interval"),
                    GUIContent.none
                );
            };
            
            // 요소 추가 시 기본값 설정
            _burstList.onAddCallback = (ReorderableList list) =>
            {
                int index = burstsProp.arraySize;
                burstsProp.InsertArrayElementAtIndex(index);
                
                // SerializedProperty 업데이트 적용
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                
                var newBurst = burstsProp.GetArrayElementAtIndex(index);
                if (newBurst != null)
                {
                    var timeProp = newBurst.FindPropertyRelative("time");
                    var cyclesProp = newBurst.FindPropertyRelative("cycles");
                    var intervalProp = newBurst.FindPropertyRelative("interval");
                    
                    if (timeProp != null) timeProp.floatValue = 0f;
                    if (cyclesProp != null) cyclesProp.intValue = 1;
                    if (intervalProp != null) intervalProp.floatValue = 0.01f;
                    
                    serializedObject.ApplyModifiedProperties();
                }
            };
            
            // 요소 높이
            _burstList.elementHeight = EditorGUIUtility.singleLineHeight + 4;
        }

        private void SetupSubEmitterList()
        {
            var subEmittersProp = _subEmitterModule.FindPropertyRelative("subEmitters");
            
            _subEmitterList = new ReorderableList(serializedObject, subEmittersProp, true, true, true, true);
            
            // 헤더 그리기
            _subEmitterList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Sub Emitter 목록");
            };
            
            // 각 요소 그리기 (한 줄씩 배치)
            _subEmitterList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = subEmittersProp.GetArrayElementAtIndex(index);
                rect.y += 2;
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float lineSpacing = 2;
                float labelWidth = EditorGUIUtility.labelWidth - 20f;
                float fieldWidth = rect.width - labelWidth;
                
                var typeProp = element.FindPropertyRelative("type");
                var subSystemProp = element.FindPropertyRelative("subParticleSystem");
                var inheritProp = element.FindPropertyRelative("inherit");
                var velocityMultProp = element.FindPropertyRelative("inheritVelocityMultiplier");
                var sizeMultProp = element.FindPropertyRelative("inheritSizeMultiplier");
                
                SubEmitterInherit inheritFlags = (SubEmitterInherit)inheritProp.intValue;
                
                // 1행: 트리거 타입
                EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, lineHeight), "트리거");
                EditorGUI.PropertyField(
                    new Rect(rect.x + labelWidth, rect.y, fieldWidth, lineHeight),
                    typeProp,
                    GUIContent.none
                );
                rect.y += lineHeight + lineSpacing;
                
                // 2행: 파티클 시스템
                EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, lineHeight), "파티클");
                EditorGUI.PropertyField(
                    new Rect(rect.x + labelWidth, rect.y, fieldWidth, lineHeight),
                    subSystemProp,
                    GUIContent.none
                );
                rect.y += lineHeight + lineSpacing;
                
                // 3행: 상속 플래그
                EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, lineHeight), "상속");
                EditorGUI.PropertyField(
                    new Rect(rect.x + labelWidth, rect.y, fieldWidth, lineHeight),
                    inheritProp,
                    GUIContent.none
                );
                rect.y += lineHeight + lineSpacing;
                
                // 4행: 속도 비율 (Velocity 플래그 상태에 따라 비활성화)
                bool velocityEnabled = (inheritFlags & SubEmitterInherit.Velocity) != 0;
                EditorGUI.BeginDisabledGroup(!velocityEnabled);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, lineHeight), "속도 비율");
                velocityMultProp.floatValue = EditorGUI.Slider(
                    new Rect(rect.x + labelWidth, rect.y, fieldWidth, lineHeight),
                    velocityMultProp.floatValue, 0f, 1f
                );
                EditorGUI.EndDisabledGroup();
                rect.y += lineHeight + lineSpacing;
                
                // 5행: 크기 비율 (Size 플래그 상태에 따라 비활성화)
                bool sizeEnabled = (inheritFlags & SubEmitterInherit.Size) != 0;
                EditorGUI.BeginDisabledGroup(!sizeEnabled);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, lineHeight), "크기 비율");
                sizeMultProp.floatValue = EditorGUI.Slider(
                    new Rect(rect.x + labelWidth, rect.y, fieldWidth, lineHeight),
                    sizeMultProp.floatValue, 0f, 2f
                );
                EditorGUI.EndDisabledGroup();
                rect.y += lineHeight + lineSpacing;
                
                // 검증 메시지 표시
                var childSystem = subSystemProp.objectReferenceValue as CanvasParticleSystem;
                if (childSystem != null)
                {
                    if (childSystem == _particle)
                    {
                        EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, lineHeight * 1.5f), "자기 자신 참조 불가", MessageType.Error);
                    }
                    else if (HasCircularReference(_particle, childSystem))
                    {
                        EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, lineHeight * 1.5f), "순환 참조 감지", MessageType.Error);
                    }
                }
            };
            
            // 요소 추가 시 기본값 설정
            _subEmitterList.onAddCallback = (ReorderableList list) =>
            {
                int index = subEmittersProp.arraySize;
                subEmittersProp.InsertArrayElementAtIndex(index);
                
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                
                var newEntry = subEmittersProp.GetArrayElementAtIndex(index);
                if (newEntry != null)
                {
                    newEntry.FindPropertyRelative("type").enumValueIndex = 0; // Birth
                    newEntry.FindPropertyRelative("subParticleSystem").objectReferenceValue = null;
                    newEntry.FindPropertyRelative("inherit").intValue = (int)(SubEmitterInherit.Position | SubEmitterInherit.Color);
                    newEntry.FindPropertyRelative("inheritVelocityMultiplier").floatValue = 0.5f;
                    newEntry.FindPropertyRelative("inheritSizeMultiplier").floatValue = 1.0f;
                    
                    serializedObject.ApplyModifiedProperties();
                }
            };
            
            // 요소 높이 계산 콜백
            _subEmitterList.elementHeightCallback = (int index) =>
            {
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float lineSpacing = 2;
                int lines = 5; // 기본 5줄 (트리거, 파티클, 상속, 속도비율, 크기비율)
                
                var element = subEmittersProp.GetArrayElementAtIndex(index);
                var subSystemProp = element.FindPropertyRelative("subParticleSystem");
                var childSystem = subSystemProp.objectReferenceValue as CanvasParticleSystem;
                
                // 검증 메시지가 있으면 추가 줄
                if (childSystem != null && (childSystem == _particle || HasCircularReference(_particle, childSystem)))
                {
                    lines += 1;
                }
                
                return lineHeight * lines + lineSpacing * lines + 6;
            };
        }

        private void DrawEmissionModule()
        {
            EditorGUILayout.PropertyField(_emissionModule.FindPropertyRelative("rateOverTime"), new GUIContent("초당 방출 개수"));
            
            GUILayout.Space(8);
            
            // ReorderableList 그리기
            if (_burstList != null)
            {
                _burstList.DoLayoutList();
            }
        }

        private void DrawShapeModule()
        {
            EditorGUILayout.PropertyField(_shapeModule.FindPropertyRelative("pivot"), new GUIContent("기준 Transform"));
            EditorGUILayout.PropertyField(_shapeModule.FindPropertyRelative("shape"), new GUIContent("방출 모양"));

            var emissionAngleProp = _shapeModule.FindPropertyRelative("emissionAngle");
            emissionAngleProp.floatValue = EditorGUILayout.FloatField("방출 각도", emissionAngleProp.floatValue);

            var shape = (EmissionShape)_shapeModule.FindPropertyRelative("shape").enumValueIndex;

            switch (shape)
            {
                case EmissionShape.Circle:
                    EditorGUILayout.PropertyField(_shapeModule.FindPropertyRelative("radius"), new GUIContent("반지름"));
                    break;
                case EmissionShape.Cone:
                    EditorGUILayout.PropertyField(_shapeModule.FindPropertyRelative("radius"), new GUIContent("반지름"));
                    var angleProp = _shapeModule.FindPropertyRelative("angle");
                    angleProp.floatValue = EditorGUILayout.FloatField("각도 범위", angleProp.floatValue);
                    break;
                case EmissionShape.Edge:
                case EmissionShape.Rectangle:
                    EditorGUILayout.PropertyField(_shapeModule.FindPropertyRelative("size"), new GUIContent("크기"));
                    break;
            }

            GUILayout.Space(8);
            
            // Spread Mode
            var miniLabelStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
            EditorGUILayout.LabelField("Spread", miniLabelStyle);
            // EditorGUI.indentLevel++;
            
            var spreadModeProp = _shapeModule.FindPropertyRelative("spreadMode");
            EditorGUILayout.PropertyField(spreadModeProp, new GUIContent("모드"));
            
            // Spread Speed (Loop, PingPong 모드일 때만 표시)
            var spreadMode = (SpreadMode)spreadModeProp.enumValueIndex;
            if (spreadMode == SpreadMode.Loop || spreadMode == SpreadMode.PingPong)
            {
                var spreadSpeedProp = _shapeModule.FindPropertyRelative("spreadSpeed");
                EditorGUILayout.Slider(spreadSpeedProp, 0.01f, 10f, new GUIContent("속도", "스프레드 순환 속도"));
            }
            
            // EditorGUI.indentLevel--;

            GUILayout.Space(6);

            // Randomness Settings
            EditorGUILayout.LabelField("Randomness", miniLabelStyle);
            // EditorGUI.indentLevel++;
            
            // Position Randomness (Circle, Cone, Rectangle에만 표시)
            if (shape != EmissionShape.Edge)
            {
                var posRandomProp = _shapeModule.FindPropertyRelative("positionRandomness");
                EditorGUILayout.Slider(posRandomProp, 0f, 1f, new GUIContent("위치 랜덤니스", "0=중앙, 1=랜덤 거리"));
            }
            
            // Direction Randomness (모든 Shape)
            var dirRandomProp = _shapeModule.FindPropertyRelative("directionRandomness");
            EditorGUILayout.Slider(dirRandomProp, 0f, 1f, new GUIContent("방향 랜덤니스", "0=위치 각도, 1=랜덤 방향"));
            
            // EditorGUI.indentLevel--;
        }

        private void DrawVelocityOverLifetimeModule()
        {
            EditorGUILayout.PropertyField(_velocityOverLifetime.FindPropertyRelative("velocityX"), new GUIContent("X"));
            EditorGUILayout.PropertyField(_velocityOverLifetime.FindPropertyRelative("velocityY"), new GUIContent("Y"));
        }

        private void DrawSizeOverLifetimeModule()
        {
            EditorGUILayout.PropertyField(_sizeOverLifetime.FindPropertyRelative("size"), new GUIContent("수명에 따른 크기"));
        }

        private void DrawColorOverLifetimeModule()
        {
            EditorGUILayout.PropertyField(_colorOverLifetime.FindPropertyRelative("color"), new GUIContent("수명에 따른 색상"));
        }

        private void DrawNoiseModule()
        {
            EditorGUILayout.PropertyField(_noiseModule.FindPropertyRelative("strength"), new GUIContent("강도"));
            EditorGUILayout.PropertyField(_noiseModule.FindPropertyRelative("frequency"), new GUIContent("주파수"));
            EditorGUILayout.PropertyField(_noiseModule.FindPropertyRelative("scrollSpeed"), new GUIContent("스크롤 속도"));
        }

        private void DrawAttractionModule()
        {
            EditorGUILayout.PropertyField(_attractionModule.FindPropertyRelative("target"), new GUIContent("대상 Transform"));
            EditorGUILayout.PropertyField(_attractionModule.FindPropertyRelative("amount"), new GUIContent("끌어당김 양 (0~1)"));
        }

        private void DrawRendererModule()
        {
            EditorGUILayout.PropertyField(_rendererModule.FindPropertyRelative("mainSprite"), new GUIContent("파티클 스프라이트"));
            EditorGUILayout.PropertyField(_rendererModule.FindPropertyRelative("maxRendererPoolSize"), new GUIContent("렌더러 풀 최대 크기"));
        }
        
        private void DrawTextureSheetAnimationModule()
        {
            var mode = _textureSheetAnimation.FindPropertyRelative("mode");
            EditorGUILayout.PropertyField(mode, new GUIContent("애니메이션 모드"));

            EditorGUILayout.PropertyField(_textureSheetAnimation.FindPropertyRelative("tilesX"), new GUIContent("타일 X"));
            EditorGUILayout.PropertyField(_textureSheetAnimation.FindPropertyRelative("tilesY"), new GUIContent("타일 Y"));
            
            EditorGUILayout.Space(5);
            
            // 모드에 따라 다른 필드 표시
            if (mode.enumValueIndex == 0) // FPS
            {
                EditorGUILayout.LabelField("FPS Mode", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_textureSheetAnimation.FindPropertyRelative("fps"), new GUIContent("초당 프레임 수"));
            }
            else // Lifetime
            {
                EditorGUILayout.LabelField("Lifetime Mode", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_textureSheetAnimation.FindPropertyRelative("cycles"), new GUIContent("반복 횟수"));
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Frame Range", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_textureSheetAnimation.FindPropertyRelative("startFrame"), new GUIContent("시작 프레임"));
            EditorGUILayout.PropertyField(_textureSheetAnimation.FindPropertyRelative("endFrame"), new GUIContent("끝 프레임 (-1: 마지막)"));
        }
        
        private void DrawSubEmitterModule(CanvasParticleSystem particleSystem)
        {
            // ReorderableList 그리기 (모든 정보가 리스트 내에 표시됨)
            if (_subEmitterList != null)
            {
                _subEmitterList.DoLayoutList();
            }
        }
        
        /// <summary>
        /// 순환 참조 체크
        /// </summary>
        private bool HasCircularReference(CanvasParticleSystem source, CanvasParticleSystem target, int depth = 0)
        {
            // 최대 depth 제한 (무한 루프 방지)
            if (depth > 10) return false;
            
            if (target == null || !target.SubEmitter.enabled) return false;
            
            foreach (var entry in target.SubEmitter.subEmitters)
            {
                if (entry.subParticleSystem == null) continue;
                
                // source를 다시 참조하는 경우
                if (entry.subParticleSystem == source)
                {
                    return true;
                }
                
                // 재귀적으로 체크
                if (HasCircularReference(source, entry.subParticleSystem, depth + 1))
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 주어진 파티클 시스템이 부모 시스템의 SubEmitter로 등록되어 있는지 확인
        /// </summary>
        private bool IsRegisteredAsSubEmitter(CanvasParticleSystem targetSystem)
        {
            return GetParentSubEmitterSystem(targetSystem) != null;
        }
        
        /// <summary>
        /// 부모 SubEmitter 시스템을 찾아 반환
        /// </summary>
        private CanvasParticleSystem? GetParentSubEmitterSystem(CanvasParticleSystem targetSystem)
        {
            if (targetSystem == null) return null;
            
            // 부모 오브젝트들을 순회하며 SubEmitter 등록 여부 확인
            Transform parent = targetSystem.transform.parent;
            while (parent != null)
            {
                var parentSystem = parent.GetComponent<CanvasParticleSystem>();
                if (parentSystem != null && parentSystem.SubEmitter.enabled)
                {
                    foreach (var entry in parentSystem.SubEmitter.subEmitters)
                    {
                        if (entry.subParticleSystem == targetSystem)
                        {
                            return parentSystem;
                        }
                    }
                }
                parent = parent.parent;
            }
            
            return null;
        }
    }
}
