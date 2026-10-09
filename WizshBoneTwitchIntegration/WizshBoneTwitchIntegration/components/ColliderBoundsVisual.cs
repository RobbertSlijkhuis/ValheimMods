using System;
using UnityEngine;

namespace WizshBoneTwitchIntegration.Components
{
    // Purely client-local visual - added to a GameObject (usually via ColliderBoundsHelper, which
    // owns the show/hide toggle and the registry of tracked objects) to show its Collider's true
    // bounds in-game as a wireframe outline (no fill, fully see-through): box edges for
    // BoxCollider, two rings + four side lines + domed end arcs for CapsuleCollider (a pill
    // outline; a capsule with no cylinder part comes out as three orthogonal rings), three
    // orthogonal rings for SphereCollider. Every line is a thin opaque
    // bar rather than a GL/LineRenderer draw, since that's simple and doesn't need a custom shader.
    // Callers that resize the collider after Initialize() (e.g. TwitchSafeZoneControls.SetRadius on
    // the ward stone) call Refresh() (or ColliderBoundsHelper.Refresh) explicitly, so this stays
    // correct without polling.
    // No ZNetView/ZDO of its own, so it never gets networked - every client only sees their own.
    internal class ColliderBoundsVisual : MonoBehaviour
    {
        private const float LineThickness = 0.05f;
        private const int RingSegments = 24;

        private GameObject m_visual;
        private Collider m_collider;
        private Material m_material;
        private Color m_color = Color.white;

        public void Awake()
        {
            m_collider = GetComponent<Collider>();
        }

        // Must be called right after AddComponent. Awake() doesn't build anything itself, so the
        // color is known before the first (and only) initial build, and the collider's final size
        // is whatever it is at this point.
        public void Initialize(Color color)
        {
            try
            {
                m_color = color;

                if (m_collider == null)
                    return;

                if (!(m_collider is BoxCollider) && !(m_collider is CapsuleCollider) && !(m_collider is SphereCollider))
                {
                    Jotunn.Logger.LogWarning($"[WBTI] ColliderBoundsVisual: unsupported collider type '{m_collider.GetType().Name}' on '{gameObject.name}'");
                    return;
                }

                Rebuild();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("ColliderBoundsVisual.Initialize failed: " + e);
            }
        }

        // Called by anything that resizes this object's collider after Initialize() already ran,
        // so the wireframe stays in sync.
        public void Refresh()
        {
            try
            {
                if (m_visual != null)
                    Rebuild();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("ColliderBoundsVisual.Refresh failed: " + e);
            }
        }

        private void Rebuild()
        {
            if (m_visual != null)
                Destroy(m_visual);

            m_visual = new GameObject("WBTI_ColliderBoundsVisual");
            m_visual.transform.SetParent(transform, false);

            // Cancel out this GameObject's own world scale on the visual root, so a line bar's
            // fixed LineThickness always renders as a true constant width - otherwise, on a ship
            // (or anything else) with non-uniform scale, a bar's thickness gets stretched by
            // whatever the scale happens to be along the world axis it's rotated to point at,
            // making some edges look much wider than others. Position/size math below re-applies
            // this same scale explicitly, component-wise, so the wireframe still matches the real
            // (scaled) collider bounds - only the line thickness itself is exempted from it.
            Vector3 scale = transform.lossyScale;
            m_visual.transform.localScale = new Vector3(
                scale.x != 0f ? 1f / scale.x : 1f,
                scale.y != 0f ? 1f / scale.y : 1f,
                scale.z != 0f ? 1f / scale.z : 1f);

            switch (m_collider)
            {
                case BoxCollider box:
                    BuildBox(box, scale);
                    break;
                case CapsuleCollider capsule:
                    BuildCapsule(capsule, scale);
                    break;
                case SphereCollider sphere:
                    BuildSphere(sphere, scale);
                    break;
            }
        }

        private void BuildBox(BoxCollider box, Vector3 scale)
        {
            Vector3 center = Vector3.Scale(box.center, scale);
            Vector3 half = Vector3.Scale(box.size, scale) / 2f;
            Vector3[] corners = new Vector3[8];

            for (int i = 0; i < 8; i++)
            {
                corners[i] = center + new Vector3(
                    (i & 1) == 0 ? -half.x : half.x,
                    (i & 2) == 0 ? -half.y : half.y,
                    (i & 4) == 0 ? -half.z : half.z);
            }

            // Index pairs into the 8 corners above - 4 edges along each axis.
            int[,] edges = new int[,]
            {
                { 0, 1 }, { 2, 3 }, { 4, 5 }, { 6, 7 },
                { 0, 2 }, { 1, 3 }, { 4, 6 }, { 5, 7 },
                { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 },
            };

            for (int i = 0; i < edges.GetLength(0); i++)
                CreateLineBar(corners[edges[i, 0]], corners[edges[i, 1]]);
        }

        // Capsule outline: a ring at each cap (where the cylinder meets the dome), 4 straight side
        // lines, and half-circle arcs over both poles in two perpendicular vertical planes.
        // Radius/height under non-uniform scale
        // are inherently approximate (there's no single "correct" scaled radius for an ellipse) -
        // this mirrors Unity's own convention of scaling a capsule's radius by its two
        // perpendicular-to-axis scale components and its height by the axis scale component.
        private void BuildCapsule(CapsuleCollider capsule, Vector3 scale)
        {
            float axisScale = capsule.direction switch { 0 => scale.x, 2 => scale.z, _ => scale.y };
            float perpScale = capsule.direction switch
            {
                0 => (scale.y + scale.z) / 2f,
                2 => (scale.x + scale.y) / 2f,
                _ => (scale.x + scale.z) / 2f,
            };

            float radius = capsule.radius * perpScale;
            float height = Mathf.Max(capsule.height * axisScale, radius * 2f);
            float halfCylinder = Mathf.Max(0f, height / 2f - radius);

            Vector3 axis = capsule.direction switch
            {
                0 => Vector3.right,
                2 => Vector3.forward,
                _ => Vector3.up,
            };

            Vector3 candidate = axis == Vector3.up ? Vector3.right : Vector3.up;
            Vector3 perpA = Vector3.Cross(axis, candidate).normalized;
            Vector3 perpB = Vector3.Cross(axis, perpA).normalized;

            Vector3 center = Vector3.Scale(capsule.center, scale);
            Vector3 topCenter = center + axis * halfCylinder;
            Vector3 bottomCenter = center - axis * halfCylinder;

            CreateRing(topCenter, perpA, perpB, radius);

            // A capsule whose height is no more than its diameter (e.g. the ward stone's zone) has
            // no cylinder part - both rings would sit on the same spot, so draw only one.
            if (halfCylinder > 0.01f)
                CreateRing(bottomCenter, perpA, perpB, radius);

            for (int i = 0; i < 4; i++)
            {
                float angle = i * Mathf.PI / 2f;
                Vector3 offset = (Mathf.Cos(angle) * perpA + Mathf.Sin(angle) * perpB) * radius;
                CreateLineBar(topCenter + offset, bottomCenter + offset);
            }

            // Domed ends: a half circle over each pole in both vertical planes. Together with the
            // horizontal ring(s) this splits a sphere-like capsule into 8 sections.
            CreateArc(topCenter, perpA, axis, radius, 0f, Mathf.PI, RingSegments / 2);
            CreateArc(topCenter, perpB, axis, radius, 0f, Mathf.PI, RingSegments / 2);
            CreateArc(bottomCenter, perpA, axis, radius, Mathf.PI, Mathf.PI * 2f, RingSegments / 2);
            CreateArc(bottomCenter, perpB, axis, radius, Mathf.PI, Mathf.PI * 2f, RingSegments / 2);
        }

        // SphereCollider radius under non-uniform scale follows Unity's own convention of using
        // the largest scale component.
        private void BuildSphere(SphereCollider sphere, Vector3 scale)
        {
            Vector3 center = Vector3.Scale(sphere.center, scale);
            float radius = sphere.radius * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));

            CreateRing(center, Vector3.right, Vector3.forward, radius);
            CreateRing(center, Vector3.right, Vector3.up, radius);
            CreateRing(center, Vector3.forward, Vector3.up, radius);
        }

        // Approximates a circle of the given radius, in the plane spanned by axisA/axisB, as a
        // closed loop of straight line bars.
        private void CreateRing(Vector3 center, Vector3 axisA, Vector3 axisB, float radius)
        {
            CreateArc(center, axisA, axisB, radius, 0f, Mathf.PI * 2f, RingSegments);
        }

        // Part of a circle in the plane spanned by axisA/axisB (angle 0 = +axisA, pi/2 = +axisB),
        // between two angles in radians, as a chain of straight line bars.
        private void CreateArc(Vector3 center, Vector3 axisA, Vector3 axisB, float radius, float startAngle, float endAngle, int segments)
        {
            Vector3 prev = center + (Mathf.Cos(startAngle) * axisA + Mathf.Sin(startAngle) * axisB) * radius;

            for (int i = 1; i <= segments; i++)
            {
                float t = Mathf.Lerp(startAngle, endAngle, i / (float)segments);
                Vector3 next = center + (Mathf.Cos(t) * axisA + Mathf.Sin(t) * axisB) * radius;
                CreateLineBar(prev, next);
                prev = next;
            }
        }

        // Default cube primitive spans 1 unit (-0.5 to 0.5) on each local axis before scaling,
        // stretched along its local Y axis into a thin bar between the two given points.
        private void CreateLineBar(Vector3 from, Vector3 to)
        {
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "Line";
            bar.transform.SetParent(m_visual.transform, false);

            Collider barCollider = bar.GetComponent<Collider>();
            if (barCollider != null)
                Destroy(barCollider);

            Vector3 mid = (from + to) / 2f;
            float length = Vector3.Distance(from, to);

            bar.transform.localPosition = mid;
            bar.transform.localRotation = Quaternion.FromToRotation(Vector3.up, (to - from).normalized);
            bar.transform.localScale = new Vector3(LineThickness, length, LineThickness);

            Renderer renderer = bar.GetComponent<Renderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = GetLineMaterial();
        }

        // Sprites/Default is a built-in Unity shader (unlit, alpha-blended, respects
        // Material.color) - Valheim's own UI depends on it, so it's safe to assume present without
        // shipping a custom shader. One material per visual (shared by all its bars), so each
        // tracked object can have its own color; destroyed again in OnDestroy.
        private Material GetLineMaterial()
        {
            if (m_material == null)
            {
                m_material = new Material(Shader.Find("Sprites/Default"));
                m_material.color = m_color;
            }

            return m_material;
        }

        public void OnDestroy()
        {
            if (m_visual != null)
                Destroy(m_visual);

            if (m_material != null)
                Destroy(m_material);
        }
    }
}
