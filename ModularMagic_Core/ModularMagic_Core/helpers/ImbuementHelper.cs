using ModularMagic_Core.Models;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class ImbuementHelper
    {
        public static Vector3 CalculatePosition(int index)
        {
            Vector3 position = new Vector3(0.3f, 2.75f, 0f);

            switch (index)
            {
                case 0:
                    position.z -= 0.45f;
                    break;
                case 1:
                    position.z -= 0.15f;
                    break;
                case 2:
                    position.z += 0.15f;
                    break;
                case 3:
                    position.z += 0.45f;
                    break;
            }

            position.y = position.y - 1.05f;

            return position;
        }

        public static List<Imbuement> StringToList(string value)
        {
            List<Imbuement> imbuements = new List<Imbuement>();

            if (string.IsNullOrEmpty(value))
                return imbuements;

            foreach (string item in value.Split(';'))
            {
                string[] properties = item.Split('|');

                if (properties.Length < 9
                    || !int.TryParse(properties[5], out int tier)
                    || !int.TryParse(properties[6], out int level)
                    || !bool.TryParse(properties[7], out bool saved))
                {
                    Jotunn.Logger.LogWarning($"[Imbuements] Skipping malformed imbuement entry: {item}");
                    continue;
                }

                imbuements.Add(new Imbuement(
                    properties[0],
                    properties[1],
                    properties[2],
                    properties[3],
                    properties[4],
                    tier,
                    level,
                    saved,
                    properties[8]
                ));
            }

            return imbuements;
        }

        public static string ListToString(List<Imbuement> imbuements)
        {
            List<string> items = new List<string>();

            foreach (Imbuement i in imbuements)
            {
                items.Add($"{Clean(i.type)}|{Clean(i.prefab)}|{Clean(i.name)}|{Clean(i.description)}|{Clean(i.value)}|{i.tier}|{i.level}|{i.saved}|{Clean(i.weaponType)}");
            }

            return string.Join(";", items);
        }

        // The delimiters of the saved string must not appear inside a field
        private static string Clean(string value)
        {
            return value == null ? "" : value.Replace('|', '/').Replace(';', ',');
        }
    }
}
