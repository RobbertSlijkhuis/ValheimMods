using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class TestCommand : ConsoleCommand
    {
        public override string Name => "WBTITest";
        public override string Help => "Start/stop random redeem stress test. Usage: WBTITest [interval] — no args stops the test.";

        private static Coroutine m_testCoroutine;
        private static int m_redeemIndex;
        private static List<string> m_enabledRedeemTitles;

        public override void Run(string[] args)
        {
            if (args.Length == 0)
            {
                StopTest();
                return;
            }

            float interval = 3f;
            if (float.TryParse(args[0], out float parsed))
                interval = parsed;

            StartTest(interval);
        }

        private void StartTest(float interval)
        {
            if (RedeemHelper.redeems == null || RedeemHelper.redeems.Count == 0)
            {
                Jotunn.Logger.LogError("No redeems are loaded, cannot start test!");
                return;
            }

            m_enabledRedeemTitles = RedeemHelper.redeems.Where(item => item.enabled).Select(item => item.title).ToList();

            if (m_enabledRedeemTitles.Count == 0)
            {
                Jotunn.Logger.LogError("No enabled redeems are loaded, cannot start test!");
                return;
            }

            if (m_testCoroutine != null)
            {
                Jotunn.Logger.LogWarning("Test already running, restarting...");
                WizshBoneTwitchIntegration.Instance.StopCoroutine(m_testCoroutine);
            }

            m_redeemIndex = 0;
            Jotunn.Logger.LogWarning($"Starting redeem stress test every {interval}s with {m_enabledRedeemTitles.Count} enabled redeems. Run 'testWBTI' with no args to stop.");
            m_testCoroutine = WizshBoneTwitchIntegration.Instance.StartCoroutine(TestLoop(interval));
        }

        private void StopTest()
        {
            if (m_testCoroutine == null)
            {
                Jotunn.Logger.LogWarning("No test is currently running.");
                return;
            }

            WizshBoneTwitchIntegration.Instance.StopCoroutine(m_testCoroutine);
            m_testCoroutine = null;
            Jotunn.Logger.LogWarning("Redeem stress test stopped.");
        }

        private IEnumerator TestLoop(float interval)
        {
            ConsoleCommand command = CommandManager.Instance.CustomCommands.FirstOrDefault(item => item.Name == "WBTIUseRedeem");

            if (command == null)
            {
                Jotunn.Logger.LogError("Could not find WBTIUseRedeem command!");
                m_testCoroutine = null;
                yield break;
            }

            while (true)
            {
                string redeem = m_enabledRedeemTitles[m_redeemIndex];
                Jotunn.Logger.LogWarning($"[TestWBTI] Firing redeem ({m_redeemIndex + 1}/{m_enabledRedeemTitles.Count}): {redeem}");
                command.Run(new string[] { redeem });

                m_redeemIndex = (m_redeemIndex + 1) % m_enabledRedeemTitles.Count;
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
