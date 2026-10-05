using Jotunn.Managers;
using System.Collections.Generic;
using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal static class TerrainEditHelper
    {
        private const float ResetRadiusBuffer = 3f;

        public static void ApplyTerrainEdit(TerrainEditData data, CustomRewardEvent customRewardEvent)
        {
            Vector3 playerPos = Player.m_localPlayer.transform.position;
            if (ZoneSystem.instance != null)
                ZoneSystem.instance.GetGroundHeight(playerPos, out playerPos.y);

            // Apply position offset relative to player facing direction
            Vector3 forward = Player.m_localPlayer.transform.forward;
            Vector3 right   = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 offset  = data.positionOffset.ToVector();
            Vector3 origin  = playerPos
                            + forward * offset.z
                            + right   * offset.x
                            + Vector3.up * offset.y;

            if (OverlapsWithSafeZone(origin, data.raiseRadius))
            {
                Jotunn.Logger.LogWarning("TerrainEditHelper: Terrain edit origin overlaps with a safe zone, aborting.");
                return;
            }

            if (data.announceMessage != null)
                Player.m_localPlayer.Message(
                    MessageHud.MessageType.Center,
                    MessageHelper.ParseVariables("{{user}}", customRewardEvent.RedeemerName, data.announceMessage),
                    3000
                );

            if (data.noFallDamage)
                Player.m_localPlayer.GetSEMan().AddStatusEffect(WizshBoneTwitchIntegration.Instance.effects.NoFallDamage);

            TerrainOp.Settings settings = BuildSettings(data);

            switch (data.shape)
            {
                case "Rectangle":
                    ApplyRectangleWall(origin, forward, settings, width: data.raiseRadius * 2f, stepSize: 1f);
                    break;
                case "Chevron":
                    ApplyChevronWall(origin, forward, settings, armLength: data.raiseRadius, armAngleDegrees: 45f, stepSize: 1f);
                    break;
                default: // Circle
                    ApplyToHeightmaps(origin, settings);
                    break;
            }

            // A fall (or a dip into water) shortly after this edit is credited to the redeemer.
            DeathCreditHelper.NoteTerrainEdit(customRewardEvent, data.noFallDamage);

            if (data.duration > 0f)
            {
                // Through ZNetViewHelper (marks it as a redeem spawn) and tagged with the redeemer, so
                // the timed reset - which can drop the streamer off raised ground - credits the same viewer.
                GameObject marker = ZNetViewHelper.Instantiate(WizshBoneTwitchIntegration.Instance.prefabs.TerrainEdit, origin, Quaternion.identity);
                RedeemerTagHelper.Apply(marker, customRewardEvent);
                marker.GetComponent<TwitchTerrainReset>().Initialize(origin, settings.GetRadius() + ResetRadiusBuffer, data.duration);
            }

            SpawnEffects(origin);
        }

        private static bool OverlapsWithSafeZone(Vector3 origin, float radius)
        {
            return TwitchSafeZone.OverlapsSafeZone(origin, radius);
        }

        private static void ApplyToHeightmaps(Vector3 pos, TerrainOp.Settings settings)
        {
            List<Heightmap> heightmaps = new List<Heightmap>();
            Heightmap.FindHeightmap(pos, settings.GetRadius(), heightmaps);

            if (heightmaps.Count == 0)
            {
                Jotunn.Logger.LogWarning("TerrainEditHelper: No Heightmap found at position.");
                return;
            }

            foreach (Heightmap heightmap in heightmaps)
                heightmap.GetAndCreateTerrainCompiler().DoOperation(pos, Vector3.zero, settings);
        }

        private static void ApplyRectangleWall(Vector3 origin, Vector3 forward, TerrainOp.Settings settings, float width, float stepSize)
        {
            // origin is the center � spread symmetrically left and right
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            int steps = Mathf.RoundToInt(width / stepSize);

            for (int i = -steps / 2; i <= steps / 2; i++)
                ApplyToHeightmaps(origin + right * (i * stepSize), settings);
        }

        private static void ApplyChevronWall(Vector3 origin, Vector3 forward, TerrainOp.Settings settings, float armLength, float armAngleDegrees, float stepSize)
        {
            Vector3 right    = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 leftDir  = (forward + Quaternion.Euler(0, -armAngleDegrees, 0) * right).normalized;
            Vector3 rightDir = (forward + Quaternion.Euler(0,  armAngleDegrees, 0) * right).normalized;

            // Shift tip back by half the arm length so the midpoint of both arms
            // sits at origin � making origin the geometric center of the chevron
            Vector3 tip   = origin - forward * (armLength / 2f);
            int     steps = Mathf.RoundToInt(armLength / stepSize);

            for (int i = 0; i <= steps; i++)
            {
                float t = i * stepSize;
                ApplyToHeightmaps(tip + leftDir  * t, settings);
                ApplyToHeightmaps(tip + rightDir * t, settings);
            }
        }

        public static void ResetTerrainToSeed(Vector3 worldPos, float radius)
        {
            List<Heightmap> heightmaps = new List<Heightmap>();
            Heightmap.FindHeightmap(worldPos, radius, heightmaps);

            foreach (Heightmap heightmap in heightmaps)
            {
                TerrainComp comp = heightmap.GetAndCreateTerrainCompiler();
                heightmap.WorldToVertex(worldPos, out int cx, out int cy);

                float   numVerts = radius / heightmap.m_scale;
                int     range    = Mathf.CeilToInt(numVerts);
                int     width    = comp.m_width + 1;
                Vector2 center   = new Vector2(cx, cy);

                for (int y = cy - range; y <= cy + range; y++)
                {
                    for (int x = cx - range; x <= cx + range; x++)
                    {
                        if (x < 0 || y < 0 || x >= width || y >= width)
                            continue;

                        if (Vector2.Distance(center, new Vector2(x, y)) > numVerts)
                            continue;

                        int idx = y * width + x;

                        comp.m_levelDelta[idx]     = 0f;
                        comp.m_smoothDelta[idx]    = 0f;
                        comp.m_modifiedHeight[idx] = false;
                        comp.m_modifiedPaint[idx]  = false;
                        comp.m_paintMask[idx]      = Color.black;
                    }
                }

                comp.Save();
                heightmap.Poke();
            }

            if (ClutterSystem.instance != null)
                ClutterSystem.instance.ResetGrass(worldPos, radius);
        }

        private static TerrainOp.Settings BuildSettings(TerrainEditData data)
        {
            return new TerrainOp.Settings
            {
                m_level            = data.level,
                m_levelOffset      = data.levelOffset,
                m_levelRadius      = data.levelRadius,
                m_square           = data.levelSquare,

                m_raise            = data.raise,
                m_raiseDelta       = data.raiseDelta,
                m_raisePower       = data.raisePower,
                m_raiseRadius      = data.raiseRadius,

                m_smooth           = data.smooth,
                m_smoothRadius     = data.smoothRadius,
                m_smoothPower      = data.smoothPower,

                m_paintCleared     = data.paintClear,
                m_paintHeightCheck = data.paintHeightCheck,
                m_paintRadius      = data.paintRadius,
                m_paintType        = ParsePaintType(data.paintType),
            };
        }

        private static TerrainModifier.PaintType ParsePaintType(string value)
        {
            if (System.Enum.TryParse(value, ignoreCase: true, out TerrainModifier.PaintType result))
                return result;

            Jotunn.Logger.LogWarning($"TerrainEditHelper: Unknown paint type '{value}', defaulting to Dirt.");
            return TerrainModifier.PaintType.Dirt;
        }

        private static void SpawnEffects(Vector3 pos)
        {
            Quaternion rot = Quaternion.identity;

            GameObject vfx = PrefabManager.Instance.GetPrefab("vfx_Place_digg");
            if (vfx != null)
                Object.Instantiate(vfx, pos, rot);

            GameObject sfx = PrefabManager.Instance.GetPrefab("sfx_rock_destroyed");
            if (sfx != null)
                Object.Instantiate(sfx, pos, rot);
        }
    }
}