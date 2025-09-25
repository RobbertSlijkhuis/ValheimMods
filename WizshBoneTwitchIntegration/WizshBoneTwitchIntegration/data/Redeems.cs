using System.Collections.Generic;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Data
{
    internal class Redeems
    {
        public List<RedeemData> list = new List<RedeemData>();

        public Redeems()
        {
            list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Greydwarf pack!", 250, RedeemColorType.Green, new List<SpawnCreatureData> { 
                new SpawnCreatureData("Greydwarf", 3, 6), 
                new SpawnCreatureData("Greydwarf_Shaman", 3, 2),
                new SpawnCreatureData("Greydwarf_Elite", 3, 3),
            }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Bat pack!", 350, RedeemColorType.Brown, new List<SpawnCreatureData> { new SpawnCreatureData("Bat", 3, 10, false, false, SpawnPositionType.Flying) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Draugr!", 350, RedeemColorType.Green, new List<SpawnCreatureData> { new SpawnCreatureData("Draugr", 3, 3) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Ulv pack!", 500, RedeemColorType.Grey, new List<SpawnCreatureData> { new SpawnCreatureData("Ulv", 1, 3) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Troll!", 500, RedeemColorType.TrollBlue, new List<SpawnCreatureData> { new SpawnCreatureData("Troll", 1, 1, false, true) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Abomination!", 750, RedeemColorType.Brown, new List<SpawnCreatureData> { new SpawnCreatureData("Abomination") }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Golem!", 750, RedeemColorType.Silver, new List<SpawnCreatureData> { new SpawnCreatureData("StoneGolem") }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Deathsquito!", 750, RedeemColorType.Yellow, new List<SpawnCreatureData> { new SpawnCreatureData("Deathsquito", 1, 1, false, false, SpawnPositionType.Flying) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Lox!", 1000, RedeemColorType.Brown, new List<SpawnCreatureData> { new SpawnCreatureData("Lox") }));

            list.Add(new RedeemData(RedeemType.ShrinkPlayer, "WBTI: Shrink Player", 500, RedeemColorType.Pink));
            list.Add(new RedeemData(RedeemType.RandomStatusEffect, "WBTI: Random buff/debuff", 500, RedeemColorType.Pink));
            // list.Add(new RedeemData(RedeemType.SpawnShower, "WBTI: Fish Detonate", 500, RedeemColorType.Red));
            list.Add(new RedeemData(RedeemType.SpawnShower, "WBTI: Fish rain", 350, RedeemColorType.Pink));
        }
    }
}
