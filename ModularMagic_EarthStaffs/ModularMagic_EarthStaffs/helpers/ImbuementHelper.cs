using ModularMagic_EarthStaffs.Models;
using ModularMagic_EarthStaffs.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using static ItemDrop;

namespace ModularMagic_EarthStaffs.Helpers
{
    internal class ImbuementHelper
    {
        public static void ApplyImbuements(ItemData itemData, string imbuementsString)
        {
            List<Imbuement> imbuements = StringToList(imbuementsString);
            ApplyImbuements(itemData, imbuements);
        }

        public static void ApplyImbuements(ItemData itemData, List<Imbuement> imbuements)
        {
            UpdateItemDataOptions options = new UpdateItemDataOptions();
            ItemData snapShotData = GetItemDataSnapshot(itemData.m_dropPrefab.name);

            if (snapShotData == null)
                throw new Exception("Snapshot ItemData is null");

            options.maxQuality = snapShotData.m_shared.m_maxQuality;
            options.timedBlockBonus = snapShotData.m_shared.m_timedBlockBonus;
            options.projectileAccuracy = snapShotData.m_shared.m_attack.m_projectileAccuracy;
            options.projectileBurst = snapShotData.m_shared.m_attack.m_burstInterval;
            options.projectileVelocity = snapShotData.m_shared.m_attack.m_projectileVel;

            foreach (Imbuement imbuement in imbuements)
            {
                if (!imbuement.enabled) 
                    continue;

                switch (imbuement.type)
                {
                    case nameof(ImbuementType.MaxQuality):
                        options.maxQuality += int.Parse(imbuement.value, CultureInfo.InvariantCulture);
                        break;
                    case nameof(ImbuementType.ParryBonus):
                        options.timedBlockBonus += float.Parse(imbuement.value, CultureInfo.InvariantCulture);
                        break;
                    case nameof(ImbuementType.ProjectileAccuracy):
                        options.projectileAccuracy -= float.Parse(imbuement.value, CultureInfo.InvariantCulture);
                        break;
                    case nameof(ImbuementType.ProjectileBurst):
                        options.projectileBurst -= float.Parse(imbuement.value, CultureInfo.InvariantCulture);
                        break;
                    case nameof(ImbuementType.ProjectileVelocity):
                        options.projectileVelocity += float.Parse(imbuement.value, CultureInfo.InvariantCulture);
                        break;
                }
            }

            Jotunn.Logger.LogWarning(options.maxQuality);
            Jotunn.Logger.LogWarning(options.timedBlockBonus);
            Jotunn.Logger.LogWarning(options.projectileAccuracy);
            Jotunn.Logger.LogWarning(options.projectileBurst);
            Jotunn.Logger.LogWarning(options.projectileVelocity);

            UpdateHelper.UpdateItemData(itemData, options);
        }

        private static ItemData GetItemDataSnapshot(string name)
        {
            switch (name)
            {
                case var value when value == ModularMagic_EarthStaffs.Instance.prefabs.staffEarth1.name:
                    return ModularMagic_EarthStaffs.Instance.snapshots.staffEarth1;
                case var value when value == ModularMagic_EarthStaffs.Instance.prefabs.staffEarth2.name:
                    return ModularMagic_EarthStaffs.Instance.snapshots.staffEarth2;
                case var value when value == ModularMagic_EarthStaffs.Instance.prefabs.staffEarth3.name:
                    return ModularMagic_EarthStaffs.Instance.snapshots.staffEarth3;
                default:
                    return null;
            }
        }

        public static List<Imbuement> StringToList(string value)
        {
            List<Imbuement> imbuements = new List<Imbuement>();
            string[] data = value.Split(',');

            Jotunn.Logger.LogWarning(data[0]);

            foreach (string item in data)
            {
                string[] properties = item.Split(':');
                imbuements.Add(new Imbuement(
                    properties[0],
                    properties[1],
                    properties[2],
                    properties[3],
                    int.Parse(properties[4]),
                    int.Parse(properties[5]),
                    new ImbuementPath(int.Parse(properties[6]), int.Parse(properties[7]), bool.Parse(properties[8])),
                    bool.Parse(properties[9]
                )));
            }

            return imbuements;
        }

        public static string ListToString(List<Imbuement> imbuements)
        {
            string items = "";

            foreach (Imbuement i in imbuements)
            {
                items += $"{i.name}:{i.type}:{i.description}:{i.value}:{i.skillRequired}:{i.materialRequired}:{i.path.column}:{i.path.row}:{i.path.allowIntersect}:{i.enabled},";
            }

            if (items != "")
                items = items.Remove(items.Length - 1);

            return items;
        }
    }
}
