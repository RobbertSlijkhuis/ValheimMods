using ModularMagic_BloodMagic.Components;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace ModularMagic_BloodMagic.Helpers
{
    /// <summary>
    /// The husks of the local player are marked on its map. Everything is read from the ZDOs on this client, so nothing is sent:
    /// - A loaded apparition of the local player that is a husk gets a marker.
    /// - A client keeps every ZDO it has received until the ZDO is destroyed (the destroy is sent to every client), also when it is far
    ///   away. So a known ZDO tells if the husk is still a husk, and a ZDO that was seen this session and is gone was destroyed: the husk
    ///   was killed or unsummoned.
    /// - The markers are saved with the character, for each world, so they are back after a relog. A saved marker whose ZDO this client
    ///   has not received yet is kept until the player is there: when its area is loaded for a few seconds and the ZDO is still not
    ///   known, the husk was removed while the player was away.
    /// </summary>
    internal static class HuskMarkerHelper
    {
        private const float TickInterval = 1f;

        // How long the area of a saved marker is loaded without its ZDO before the marker is removed. It gives the server the time to send
        // the ZDOs of the area
        private const float MissingTime = 5f;

        private const string PinName = "Banished apparition";
        private const Minimap.PinType PinIcon = Minimap.PinType.Icon3;
        private const string SaveKeyPrefix = "MMBM_Husks_";

        private class Marker
        {
            public Vector3 position;
            public Minimap.PinData? pin;

            // True when this client has had the ZDO of the husk this session
            public bool seen;

            // The time the area of the marker was first seen loaded without the ZDO, -1 when it is not
            public float missingSince = -1f;
        }

        private static readonly Dictionary<ZDOID, Marker> markers = new Dictionary<ZDOID, Marker>();
        private static readonly List<ZDOID> removed = new List<ZDOID>();

        // The character and the world the markers were loaded for. After a death the character gets a new Player object
        private static Player? loadedPlayer;
        private static long loadedPlayerID;
        private static string saveKey = "";

        private static float nextTick;

        /// <summary>Called every frame by the plugin, does its work once every TickInterval.</summary>
        public static void Update()
        {
            if (Minimap.instance == null || ZNet.instance == null || ZDOMan.instance == null || ZNetScene.instance == null)
            {
                // Logged out: the map and its pins are gone with the game
                if (loadedPlayerID != 0L || markers.Count > 0)
                {
                    markers.Clear();
                    loadedPlayer = null;
                    loadedPlayerID = 0L;
                    saveKey = "";
                }

                return;
            }

            // Dead or not spawned yet: the markers stay on the map. The ID is 0 until the Player has its ZDO data
            Player player = Player.m_localPlayer;
            if (player == null || player.GetPlayerID() == 0L)
                return;

            if (Time.time < nextTick)
                return;

            nextTick = Time.time + TickInterval;

            if (loadedPlayer != player)
            {
                string key = GetSaveKey();

                // The same character after a death: the markers are kept and written to the data of its new Player object
                if (player.GetPlayerID() == loadedPlayerID && key == saveKey)
                {
                    loadedPlayer = player;
                    Save(player);
                }
                else
                {
                    Load(player, key);
                }
            }

            bool changed = AddLoadedHusks(player.GetPlayerID());
            changed |= UpdateMarkers();

            if (changed)
                Save(player);

            UpdatePins();
        }

        // A loaded apparition of the local player that is a husk gets a marker
        private static bool AddLoadedHusks(long playerID)
        {
            bool changed = false;

            foreach (ApparitionController apparition in ApparitionHelper.Loaded)
            {
                if (apparition == null || apparition.Summoner != playerID || !apparition.IsHusk)
                    continue;

                ZDO? zdo = apparition.Character != null && apparition.Character.m_nview != null ? apparition.Character.m_nview.GetZDO() : null;

                if (zdo == null || markers.ContainsKey(zdo.m_uid))
                    continue;

                markers[zdo.m_uid] = new Marker { position = zdo.GetPosition(), seen = true };
                changed = true;
            }

            return changed;
        }

        // Every marker follows the ZDO of its husk, see the summary of the class
        private static bool UpdateMarkers()
        {
            removed.Clear();
            bool changed = false;

            foreach (KeyValuePair<ZDOID, Marker> pair in markers)
            {
                Marker marker = pair.Value;
                ZDO? zdo = ZDOMan.instance.GetZDO(pair.Key);

                if (zdo != null)
                {
                    marker.seen = true;
                    marker.missingSince = -1f;

                    // Reanimated
                    if (zdo.GetFloat(ZDOVars.s_health, 1f) > 0f)
                    {
                        removed.Add(pair.Key);
                        continue;
                    }

                    Vector3 position = zdo.GetPosition();

                    if (Vector3.Distance(position, marker.position) > 0.5f)
                    {
                        marker.position = position;
                        changed = true;
                    }

                    continue;
                }

                // Seen this session and gone: destroyed, a ZDO that is only far away is kept by the client
                if (marker.seen)
                {
                    removed.Add(pair.Key);
                    continue;
                }

                // Only known from the save: removed when its area is loaded for a while and the ZDO did not come
                if (!ZNetScene.instance.IsAreaReady(marker.position))
                {
                    marker.missingSince = -1f;
                    continue;
                }

                if (marker.missingSince < 0f)
                    marker.missingSince = Time.time;
                else if (Time.time - marker.missingSince >= MissingTime)
                    removed.Add(pair.Key);
            }

            foreach (ZDOID id in removed)
            {
                RemovePin(markers[id]);
                markers.Remove(id);
                changed = true;
            }

            return changed;
        }

        // Every marker has a pin on the map. A pin that was removed by hand comes back while the husk is there
        private static void UpdatePins()
        {
            foreach (Marker marker in markers.Values)
            {
                if (marker.pin == null || !Minimap.instance.m_pins.Contains(marker.pin))
                    marker.pin = Minimap.instance.AddPin(marker.position, PinIcon, PinName, save: false, isChecked: false);
                else
                    marker.pin.m_pos = marker.position;
            }
        }

        private static void RemovePin(Marker marker)
        {
            if (marker.pin != null && Minimap.instance != null)
                Minimap.instance.RemovePin(marker.pin);

            marker.pin = null;
        }

        // The markers of this character in this world, as "userID,id,x,y,z" separated by ';'
        private static string GetSaveKey()
        {
            return SaveKeyPrefix + ZNet.instance.GetWorldUID().ToString(CultureInfo.InvariantCulture);
        }

        private static void Load(Player player, string key)
        {
            foreach (Marker marker in markers.Values)
                RemovePin(marker);

            markers.Clear();
            loadedPlayer = player;
            loadedPlayerID = player.GetPlayerID();
            saveKey = key;

            if (!player.m_customData.TryGetValue(saveKey, out string data) || string.IsNullOrEmpty(data))
                return;

            foreach (string entry in data.Split(';'))
            {
                string[] parts = entry.Split(',');

                if (parts.Length != 5
                    || !long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long userID)
                    || !uint.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out uint id)
                    || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                    || !float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float y)
                    || !float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    Jotunn.Logger.LogWarning($"[Apparition] Could not read the husk marker '{entry}'");
                    continue;
                }

                markers[new ZDOID(userID, id)] = new Marker { position = new Vector3(x, y, z) };
            }

            Jotunn.Logger.LogWarning($"[Apparition] Loaded {markers.Count} husk marker(s)");
        }

        private static void Save(Player player)
        {
            if (markers.Count == 0)
            {
                player.m_customData.Remove(saveKey);
                return;
            }

            StringBuilder data = new StringBuilder();

            foreach (KeyValuePair<ZDOID, Marker> pair in markers)
            {
                if (data.Length > 0)
                    data.Append(';');

                Vector3 position = pair.Value.position;
                data.Append(pair.Key.UserID.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(pair.Key.ID.ToString(CultureInfo.InvariantCulture)).Append(',')
                    .Append(position.x.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                    .Append(position.y.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                    .Append(position.z.ToString("0.##", CultureInfo.InvariantCulture));
            }

            player.m_customData[saveKey] = data.ToString();
        }
    }
}
