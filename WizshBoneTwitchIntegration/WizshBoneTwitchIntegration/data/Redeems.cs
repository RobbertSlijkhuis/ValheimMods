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
            //list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Greydwarf pack!", 250, RedeemColorType.Green, false, new List<SpawnCreatureData> { 
            //    new SpawnCreatureData("Greydwarf", 3, 6), 
            //    new SpawnCreatureData("Greydwarf_Shaman", 3, 2),
            //    new SpawnCreatureData("Greydwarf_Elite", 3, 3),
            //}));
            //list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Bat pack!", 350, RedeemColorType.Brown, false, new List<SpawnCreatureData> { new SpawnCreatureData("Bat", 3, 10, false, false, SpawnPositionType.Flying) }));
            //list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Draugr!", 350, RedeemColorType.Green, false, new List<SpawnCreatureData> { new SpawnCreatureData("Draugr", 3, 5) }));
            //list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Ulv pack!", 500, RedeemColorType.Grey, false, new List<SpawnCreatureData> { new SpawnCreatureData("Ulv", 1, 3) }));
            //list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Troll!", 500, RedeemColorType.TrollBlue, false, new List<SpawnCreatureData> { new SpawnCreatureData("Troll") }));
            //list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Troll + message!", 500, RedeemColorType.TrollBlue, true, new List<SpawnCreatureData> { new SpawnCreatureData("Troll", 1, 1, false, true) }));
            //list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Abomination!", 750, RedeemColorType.Brown, false, new List<SpawnCreatureData> { new SpawnCreatureData("Abomination") }));
            //list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Golem!", 750, RedeemColorType.Silver, false, new List<SpawnCreatureData> { new SpawnCreatureData("StoneGolem") }));
            //list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Deathsquito!", 750, RedeemColorType.Yellow, false, new List<SpawnCreatureData> { new SpawnCreatureData("Deathsquito", 1, 1, false, false, SpawnPositionType.Flying) }));
            //list.Add(new RedeemData(RedeemType.SpawnCreature, "WBTI: Spawn Lox!", 1000, RedeemColorType.Brown, false, new List<SpawnCreatureData> { new SpawnCreatureData("Lox") }));

            //// list.Add(new RedeemData(RedeemType.SpawnHallucination, "WBTI: Spawn Hallucinations!", 350, RedeemColorType.Red));

            //list.Add(new RedeemData(RedeemType.SpawnShower, "WBTI: Fish rain", 350, RedeemColorType.Pink));
            //list.Add(new RedeemData(RedeemType.ExplodeFish, "WBTI: Fish Detonate", 1000, RedeemColorType.Red));
            //list.Add(new RedeemData(RedeemType.PlayerShrink, "WBTI: Shrink Player", 500, RedeemColorType.Pink));
            //list.Add(new RedeemData(RedeemType.PlayerGrow, "WBTI: Grow Player", 500, RedeemColorType.Pink));
            //list.Add(new RedeemData(RedeemType.StatusEffectRandom, "WBTI: Random buff/debuff", 500, RedeemColorType.Pink));
            //list.Add(new RedeemData(RedeemType.TerrainRemove, "WBTI: Remove the country", 1000, RedeemColorType.Pink));

            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Neck!", 1, RedeemColorType.LightGreen, 0, false, null, new List<SpawnCreatureData> { new SpawnCreatureData("Neck", 1, 1) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Greyling!", 1, RedeemColorType.LightGreen, 0, false, null, new List<SpawnCreatureData> { new SpawnCreatureData("Greyling", 1, 1) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Boar!", 1, RedeemColorType.LightGreen, 0, false, null, new List<SpawnCreatureData> { new SpawnCreatureData("Boar", 1, 1) }));

            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Greydwarf!", 1, RedeemColorType.Green, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Greydwarf", 1, 1)}));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Greydwarf Elite!", 1, RedeemColorType.Green, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Greydwarf_Elite", 1, 1)}));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Greydwarf Shaman", 1, RedeemColorType.Green, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Greydwarf_Shaman", 1, 1)}));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Ghost", 1, RedeemColorType.Green, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Ghost", 1, 1)}));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Skeleton", 1, RedeemColorType.Green, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Skeleton", 1, 1)}));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Troll!", 1, RedeemColorType.TrollBlue, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Troll") }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Troll + message!", 1, RedeemColorType.TrollBlue, 0, true, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Troll", 1, 1, false, true) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Bat!", 1, RedeemColorType.Silver, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Bat", 1, 1, false, false, SpawnPositionType.Flying) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Draugr!", 1, RedeemColorType.Brown, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Draugr", 1, 5) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Abomination!", 1, RedeemColorType.Brown, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Abomination") }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Ulv!", 1, RedeemColorType.Silver, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("Ulv", 1, 1) }));
            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Stone Golem!", 1, RedeemColorType.Silver, 0, false, GlobalKeyType.DefeatedEikthyr, new List<SpawnCreatureData> { new SpawnCreatureData("StoneGolem") }));

            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Deathsquito!", 1, RedeemColorType.Yellow, 0, false, GlobalKeyType.DefeatedBonemass, new List<SpawnCreatureData> { new SpawnCreatureData("Deathsquito", 1, 1, false, false, SpawnPositionType.Flying) }));

            list.Add(new RedeemData(RedeemType.SpawnCreature, "Spawn: Lox!", 1, RedeemColorType.Yellow, 0, false, GlobalKeyType.DefeatedModer, new List<SpawnCreatureData> { new SpawnCreatureData("Lox") }));

            list.Add(new RedeemData(RedeemType.SpawnShower, "Misc: Fish Rain", 1, RedeemColorType.Pink));
            list.Add(new RedeemData(RedeemType.ExplodeFish, "Misc: Fish Detonate", 1, RedeemColorType.Red));
            list.Add(new RedeemData(RedeemType.PlayerShrink, "Misc: Player shrink", 1, RedeemColorType.Pink));
            list.Add(new RedeemData(RedeemType.PlayerGrow, "Misc: Player Grow", 1, RedeemColorType.Pink));
            list.Add(new RedeemData(RedeemType.StatusEffectRandom, "Misc: Random buff/debuff", 1, RedeemColorType.Pink, 60));
            list.Add(new RedeemData(RedeemType.TerrainRemove, "Misc: Remove the country", 1, RedeemColorType.Pink, 0, false, GlobalKeyType.DefeatedEikthyr));
        }
    }
}
