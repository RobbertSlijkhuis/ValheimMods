using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    // Standalone reimplementation of the vanilla ShieldDomeImageEffect (the Ashlands
    // piece_shieldgenerator dome). Reuses the same shader but keeps its own registry so
    // it never needs a real ShieldGenerator and can give each registered dome its own color.
    // Domes are grouped by color and drawn in separate Blit passes, so same-colored domes
    // (the same "type") merge smoothly while different-colored domes never blend together.
    internal class TwitchShieldDomeEffect : MonoBehaviour
    {
        private struct DomeEntry
        {
            public Vector3 position;
            public float radius;
            public Color color;
            public float lastHitTime;
        }

        private struct ShieldDomeData
        {
            public Vector3 position;
            public float radius;
            public float fuelFactor;
            public float lastHitTime;
        }

        private const int ShieldDomeStride = 24;

        // Tunable look - edit and rebuild to experiment. Defaults below are what the live
        // vanilla ShieldDomeImageEffect instance actually used in-game (confirmed via log),
        // except where noted as a code-level vanilla default (no in-scene override observed).
        private const int MaxSteps = 256;              // raymarch step count (vanilla computes this dynamically off graphics settings; 256 is just a safe high guess)
        private const float Smoothing = 0.25f;          // vanilla code default; how smoothly overlapping same-color domes blend into each other
        private const float DepthFadeDistance = 3f;     // vanilla code default; how quickly the dome fades in as the camera approaches it
        private const float EdgeGlowDistance = 1f;      // vanilla code default; width of the bright glow at the dome's silhouette edge
        private const float DrawDistance = 100f;        // vanilla code default; max raymarch distance
        private const float SurfaceDistance = 0.001f;   // vanilla code default; raymarch hit threshold (smaller = sharper but more expensive)
        private const float NormalBias = 0.1f;          // vanilla code default; offset used when estimating surface normals for lighting/refraction
        private const float RefractStrength = 1f;       // vanilla scene value; how strongly the background is bent/warped through the dome
        private const float NoiseSize = 15f;             // vanilla code default; world-space scale of the (non-curl) noise pattern
        private const float CurlSize = 16.5f;            // vanilla scene value (no code-level default); world-space scale of the curl-noise swirl pattern
        private const float CurlStrength = 0.2f;         // vanilla scene value (no code-level default); how strongly the curl noise distorts the surface

        // _NoiseSize/_CurlNoiseSize above are fixed world-space uniforms tuned for vanilla's
        // narrow 10-30m shield range. Our domes range much wider (a 3m Ward to a huge TimeStop
        // zone), so a fixed value reads as smooth/flat on small domes (pattern coarser than the
        // whole surface) - scale it with each dome's own radius, relative to vanilla's rough
        // midpoint, so the pattern keeps a consistent visual density regardless of absolute size.
        private const float ReferenceRadius = 20f;

        private static readonly int s_topLeft = Shader.PropertyToID("_TopLeft");
        private static readonly int s_topRight = Shader.PropertyToID("_TopRight");
        private static readonly int s_bottomLeft = Shader.PropertyToID("_BottomLeft");
        private static readonly int s_bottomRight = Shader.PropertyToID("_BottomRight");
        private static readonly int s_smoothing = Shader.PropertyToID("_Smoothing");
        private static readonly int s_depthFade = Shader.PropertyToID("_DepthFade");
        private static readonly int s_edgeGlow = Shader.PropertyToID("_EdgeGlow");
        private static readonly int s_drawDistance = Shader.PropertyToID("_DrawDistance");
        private static readonly int s_domeBuffer = Shader.PropertyToID("_DomeBuffer");
        private static readonly int s_domeCount = Shader.PropertyToID("_DomeCount");
        private static readonly int s_maxSteps = Shader.PropertyToID("_MaxSteps");
        private static readonly int s_surfaceDistance = Shader.PropertyToID("_SurfaceDistance");
        private static readonly int s_normalBias = Shader.PropertyToID("_NormalBias");
        private static readonly int s_refractStrength = Shader.PropertyToID("_RefractStrength");
        private static readonly int s_shieldColorGradient = Shader.PropertyToID("_ShieldColorGradient");
        private static readonly int s_shieldTime = Shader.PropertyToID("_ShieldTime");
        private static readonly int s_noiseTexture = Shader.PropertyToID("_NoiseTexture");
        private static readonly int s_curlNoiseTexture = Shader.PropertyToID("_CurlNoiseTexture");
        private static readonly int s_noiseSize = Shader.PropertyToID("_NoiseSize");
        private static readonly int s_curlNoiseSize = Shader.PropertyToID("_CurlNoiseSize");
        private static readonly int s_curlNoiseStrength = Shader.PropertyToID("_CurlNoiseStrength");

        private readonly Dictionary<object, DomeEntry> m_domes = new Dictionary<object, DomeEntry>();
        private readonly Dictionary<Color, Texture2D> m_colorTextures = new Dictionary<Color, Texture2D>();

        private Camera m_cam;
        private Material m_material;
        private ComputeBuffer m_buffer;
        private int m_bufferCount = -1;

        // Only the actual noise texture assets are pulled from the live vanilla instance -
        // there's no shipping our own, and they're not simple values to "play with" like the
        // constants above.
        private Texture2D m_noiseTexture;
        private Texture3D m_curlNoiseTexture;

        private bool m_copiedVanillaTextures;

        public void Awake()
        {
            try
            {
                m_cam = GetComponent<Camera>();
                m_material = new Material(Shader.Find("Hidden/ShieldDomePass"));
                enabled = false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchShieldDomeEffect.Awake failed: " + e);
            }
        }

        // The noise textures are plain public fields with no runtime setter, so a one-shot
        // copy would be enough in theory - but retrying here every frame until it succeeds
        // sidesteps any Awake-order/lookup timing issue (e.g. the vanilla component's
        // GameObject being inactive at the moment we first looked) instead of silently
        // keeping them null.
        private void RefreshVanillaTextures()
        {
            if (m_copiedVanillaTextures)
                return;

            ShieldDomeImageEffect vanilla = UnityEngine.Object.FindFirstObjectByType<ShieldDomeImageEffect>(FindObjectsInactive.Include);
            if (vanilla == null)
                return;

            m_noiseTexture = vanilla.m_noiseTexture;
            m_curlNoiseTexture = vanilla.m_curlNoiseTexture;

            m_copiedVanillaTextures = true;
        }

        public void SetDome(object key, Vector3 position, float radius, Color color, float lastHitTime = -1000f)
        {
            m_domes[key] = new DomeEntry { position = position, radius = radius, color = color, lastHitTime = lastHitTime };
            enabled = true;
        }

        public void RemoveDome(object key)
        {
            if (m_domes.Remove(key))
                enabled = m_domes.Count > 0;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            try
            {
                if (m_domes.Count == 0)
                {
                    Graphics.Blit(src, dest);
                    return;
                }

                if (m_cam == null)
                    m_cam = GetComponent<Camera>();

                RefreshVanillaTextures();

                m_material.SetVector(s_topLeft, m_cam.ViewportPointToRay(new Vector3(0f, 1f, 0f)).direction);
                m_material.SetVector(s_topRight, m_cam.ViewportPointToRay(new Vector3(1f, 1f, 0f)).direction);
                m_material.SetVector(s_bottomLeft, m_cam.ViewportPointToRay(new Vector3(0f, 0f, 0f)).direction);
                m_material.SetVector(s_bottomRight, m_cam.ViewportPointToRay(new Vector3(1f, 0f, 0f)).direction);
                m_material.SetFloat(s_smoothing, Smoothing);
                m_material.SetFloat(s_depthFade, DepthFadeDistance);
                m_material.SetFloat(s_edgeGlow, EdgeGlowDistance);
                m_material.SetFloat(s_drawDistance, DrawDistance);
                m_material.SetFloat(s_shieldTime, Time.time);
                m_material.SetTexture(s_noiseTexture, m_noiseTexture);
                m_material.SetTexture(s_curlNoiseTexture, m_curlNoiseTexture);
                m_material.SetFloat(s_curlNoiseStrength, CurlStrength);
                m_material.SetInt(s_maxSteps, MaxSteps);
                m_material.SetFloat(s_surfaceDistance, SurfaceDistance);
                m_material.SetFloat(s_normalBias, NormalBias);
                m_material.SetFloat(s_refractStrength, RefractStrength);

                List<IGrouping<Color, DomeEntry>> groups = m_domes.Values.GroupBy(d => d.color).ToList();

                RenderTexture current = src;
                for (int i = 0; i < groups.Count; i++)
                {
                    DomeEntry[] group = groups[i].ToArray();
                    bool isLast = i == groups.Count - 1;
                    RenderTexture target = isLast ? dest : RenderTexture.GetTemporary(src.width, src.height, src.depth, src.format);

                    UploadBuffer(group);
                    m_material.SetTexture(s_shieldColorGradient, GetColorTexture(groups[i].Key));
                    m_material.SetBuffer(s_domeBuffer, m_buffer);
                    m_material.SetInt(s_domeCount, group.Length);

                    float radiusScale = group[0].radius / ReferenceRadius;
                    m_material.SetFloat(s_noiseSize, NoiseSize * radiusScale);
                    m_material.SetFloat(s_curlNoiseSize, CurlSize * radiusScale);

                    Graphics.Blit(current, target, m_material, 0);

                    if (current != src)
                        RenderTexture.ReleaseTemporary(current);

                    current = target;
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchShieldDomeEffect.OnRenderImage failed: " + e);
                Graphics.Blit(src, dest);
            }
        }

        private void UploadBuffer(DomeEntry[] group)
        {
            if (m_buffer == null || m_bufferCount != group.Length)
            {
                m_buffer?.Release();
                m_buffer = new ComputeBuffer(group.Length, ShieldDomeStride, ComputeBufferType.Structured);
                m_bufferCount = group.Length;
            }

            ShieldDomeData[] data = new ShieldDomeData[group.Length];
            for (int i = 0; i < group.Length; i++)
            {
                data[i] = new ShieldDomeData
                {
                    position = group[i].position,
                    radius = group[i].radius,
                    // Irrelevant to our solid-color 1x1 gradient texture lookup; ruled out as the
                    // cause of the missing distortion (vanilla shields swirl at full fuel too).
                    fuelFactor = 1f,
                    lastHitTime = group[i].lastHitTime
                };
            }

            m_buffer.SetData(data);
        }

        private Texture2D GetColorTexture(Color color)
        {
            if (m_colorTextures.TryGetValue(color, out Texture2D texture))
                return texture;

            texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            m_colorTextures[color] = texture;
            return texture;
        }

        public void OnDestroy()
        {
            try
            {
                m_buffer?.Release();
                m_buffer = null;

                if (m_material != null)
                    UnityEngine.Object.Destroy(m_material);

                foreach (Texture2D texture in m_colorTextures.Values)
                {
                    if (texture != null)
                        UnityEngine.Object.Destroy(texture);
                }

                m_colorTextures.Clear();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("TwitchShieldDomeEffect.OnDestroy failed: " + e);
            }
        }
    }
}
