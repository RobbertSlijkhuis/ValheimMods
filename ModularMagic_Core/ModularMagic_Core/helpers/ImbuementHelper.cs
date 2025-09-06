using ModularMagic_Core.Models;
using ModularMagic_EarthStaffs.Types;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class ImbuementHelper
    {
        public static Vector3 CalculatePosition(string category, int column)
        {
            Vector3 position = new Vector3(0.3f, 2.75f, 0f);

            switch (column)
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

            switch (category)
            {
                case nameof(ImbuementCategoryType.Normal):
                    position.y = position.y - 1.05f;
                    break;
                case nameof(ImbuementCategoryType.Attack):
                    position.y = position.y - 0.7f;
                    break;
                case nameof(ImbuementCategoryType.SecondaryAttack):
                    position.y = position.y - 0.35f;
                    break;
            }

            return position;
        }

        public static RuneMaterials GetRuneMaterialByInteger(int integer)
        {
            switch (integer)
            {
                case 0:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneWoodOffA, ModularMagic_Core.Instance.materials.RuneWoodA, ModularMagic_Core.Instance.materials.RuneStoneA, ModularMagic_Core.Instance.materials.RuneMarbleA, ModularMagic_Core.Instance.materials.RuneGraustenA);
                case 1:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneWoodOffB, ModularMagic_Core.Instance.materials.RuneWoodB, ModularMagic_Core.Instance.materials.RuneStoneB, ModularMagic_Core.Instance.materials.RuneMarbleB, ModularMagic_Core.Instance.materials.RuneGraustenB);
                case 2:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneWoodOffC, ModularMagic_Core.Instance.materials.RuneWoodC, ModularMagic_Core.Instance.materials.RuneStoneC, ModularMagic_Core.Instance.materials.RuneMarbleC, ModularMagic_Core.Instance.materials.RuneGraustenC);
                case 3:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneWoodOffD, ModularMagic_Core.Instance.materials.RuneWoodD, ModularMagic_Core.Instance.materials.RuneStoneD, ModularMagic_Core.Instance.materials.RuneMarbleD, ModularMagic_Core.Instance.materials.RuneGraustenD);
                case 4:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneWoodOffE, ModularMagic_Core.Instance.materials.RuneWoodE, ModularMagic_Core.Instance.materials.RuneStoneE, ModularMagic_Core.Instance.materials.RuneMarbleE, ModularMagic_Core.Instance.materials.RuneGraustenE);
                case 5:
                    return new RuneMaterials(ModularMagic_Core.Instance.materials.RuneWoodOffF, ModularMagic_Core.Instance.materials.RuneWoodF, ModularMagic_Core.Instance.materials.RuneStoneF, ModularMagic_Core.Instance.materials.RuneMarbleF, ModularMagic_Core.Instance.materials.RuneGraustenF);
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
                    properties[5],
                    int.Parse(properties[6]),
                    int.Parse(properties[7]),
                    bool.Parse(properties[8])
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
                items += $"{i.name}|{i.description}|{i.type}|{i.category}|{i.column}|{i.value}|{i.level}|{i.maxLevel}|{i.enabled};";
            }

            if (items != "")
                items = items.Remove(items.Length - 1);

            return items;
        }
    }
}
