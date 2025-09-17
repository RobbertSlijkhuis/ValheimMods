using Jotunn.Managers;
using System.Collections.Generic;
using TwitchSDK;
using TwitchSDK.Interop;
using UnityEngine;

namespace WizshBoneTwitchIntegration.TwitchIntegration
{
    internal class TwitchCustomRewards : MonoBehaviour
    {
        GameTask<EventStream<CustomRewardEvent>> CustomRewardEvents;

        // Start is called before the first frame update
        void Start()
        {
            CustomRewardEvents = Twitch.API.SubscribeToCustomRewardEvents();
        }

        // Update is called once per frame
        void Update()
        {
            CustomRewardEvent CurRewardEvent;
            CustomRewardEvents.MaybeResult.TryGetNextEvent(out CurRewardEvent);
            if (CurRewardEvent != null)
            {
                // Do something
                Jotunn.Logger.LogWarning($"{CurRewardEvent.RedeemerName} has bought {CurRewardEvent.CustomRewardTitle} for {CurRewardEvent.CustomRewardCost}!");

                if (CurRewardEvent.CustomRewardTitle == "WBTI: Spawn Troll!")
                {
                    Transform transform = Player.m_localPlayer.transform;
                    GameObject prefab = PrefabManager.Instance.GetPrefab("Troll");

                    if (prefab == null)
                    {
                        Jotunn.Logger.LogError("Could not find prefab to spawn");
                    }
                        
                    GameObject troll = Object.Instantiate(prefab, transform.position, transform.rotation);
                    Humanoid humanComp = troll.GetComponent<Humanoid>();
                    humanComp.m_name = CurRewardEvent.RedeemerName;
                }
            }
        }

        public void SetSampleRewards()
        {
            CustomRewardDefinition spawnGreydwarfPack = new CustomRewardDefinition();
            spawnGreydwarfPack.Title = "WBTI: Spawn Greydwarf pack!";
            spawnGreydwarfPack.Cost = 200;

            CustomRewardDefinition spawnTroll = new CustomRewardDefinition();
            spawnTroll.Title = "WBTI: Spawn Troll!";
            spawnTroll.Cost = 250;


            List<CustomRewardDefinition> cDefinitionList = new List<CustomRewardDefinition>();
            // cDefinitionList.Add(spawnGreydwarfPack);
            cDefinitionList.Add(spawnTroll);

            Twitch.API.ReplaceCustomRewards(cDefinitionList.ToArray());
        }

        public void ClearRewards()
        {
            CustomRewardDefinition Cleared = new CustomRewardDefinition();
            Cleared.Title = "";
            Cleared.Cost = 0;
            Cleared.IsEnabled = false;
            Twitch.API.ReplaceCustomRewards(Cleared);
        }
    }
}
