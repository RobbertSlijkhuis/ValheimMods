using ModularMagic_Core.Models;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class ImbuementHelper
    {
        public static Vector3 CalculatePosition(ImbuementPath path)
        {
            Vector3 position = new Vector3(0.3f, 2.75f, 0f);
            
            switch (path.column)
            {
                case 1:
                    position.z = position.z - 0.45f;
                    break;
                case 2:
                    position.z = position.z - 0.15f;
                    break;
                case 3:
                    position.z = position.z + 0.15f;
                    break;
                case 4:
                    position.z = position.z + 0.45f;
                    break;
            }

            switch (path.row)
            {
                case 2:
                    position.y = position.y - 0.35f;
                    break;
                case 3:
                    position.y = position.y - 0.7f;
                    break;
                case 4:
                    position.y = position.y - 1.05f;
                    break;
                case 5:
                    position.y = position.y - 1.4f;
                    break;
            }

            return position;
        }

        public static RuneMaterials GetRuneMaterialByInteger(int integer)
        {
            switch (integer)
            {
                case 0:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneA, ModularMagic_Core.Instance.materials.RuneEmissiveA);
                case 1:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneB, ModularMagic_Core.Instance.materials.RuneEmissiveB);
                case 2:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneC, ModularMagic_Core.Instance.materials.RuneEmissiveC);
                case 3:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneD, ModularMagic_Core.Instance.materials.RuneEmissiveD);
                case 4:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneE, ModularMagic_Core.Instance.materials.RuneEmissiveE);
                case 5:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneF, ModularMagic_Core.Instance.materials.RuneEmissiveF);
                default:
                    return null;
            }
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
                    int.Parse(properties[4]),
                    int.Parse(properties[5]),
                    new ImbuementPath(int.Parse(properties[6]), int.Parse(properties[7]), bool.Parse(properties[8])),
                    bool.Parse(properties[9])
                );
                imbuement.isImbued = imbuement.enabled;
                imbuements.Add(imbuement);
            }

            return imbuements;
        }

        public static string ListToString(List<Imbuement> imbuements)
        {
            string items = "";

            foreach (Imbuement i in imbuements)
            {
                items += $"{i.name}|{i.type}|{i.description}|{i.value}|{i.skillRequired}|{i.materialRequired}|{i.path.column}|{i.path.row}|{i.path.allowIntersect}|{i.enabled};";
            }

            if (items != "")
                items = items.Remove(items.Length - 1);

            return items;
        }
    }
}
