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
            string[] data = value.Split(';');

            foreach (string item in data)
            {
                string[] properties = item.Split('|');
                Imbuement imbuement = new Imbuement(
                    properties[0],
                    properties[1],
                    properties[2],
                    properties[3],
                    properties[4],
                    int.Parse(properties[5]),
                    bool.Parse(properties[6]),
                    bool.Parse(properties[7]),
                    properties[8],
                    bool.Parse(properties[9])
                );

                imbuements.Add(imbuement);
            }

            return imbuements;
        }

        public static string ListToString(List<Imbuement> imbuements)
        {
            string items = "";

            foreach (Imbuement i in imbuements)
            {
                items += $"{i.type}|{i.prefab}|{i.name}|{i.description}|{i.value}|{i.level}|{i.charged}|{i.saved}|{i.weaponType}|{i.allowSecondary};";
            }

            if (items != "")
                items = items.Remove(items.Length - 1);

            return items;
        }
    }
}
