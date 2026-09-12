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
        public override string Help => "Start/stop random redeem stress test. Usage: WBTITest [interval] [loops] — loops defaults to 1 (never runs indefinitely) and stops the test automatically after cycling through every enabled redeem that many times; no args stops a running test early.";

        // No indefinite mode by design - if the test lags the game badly enough, stopping it manually
        // (re-running this command with no args) can be difficult/slow to even get a keypress through.
        // Defaulting loops to 1 means a plain "WBTITest <interval>" always self-terminates.
        private const int DefaultLoops = 1;

        private static Coroutine m_testCoroutine;
        private static int m_redeemIndex;
        private static int m_loopsCompleted;
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

            int maxLoops = DefaultLoops;
            if (args.Length > 1 && int.TryParse(args[1], out int parsedLoops) && parsedLoops > 0)
                maxLoops = parsedLoops;

            StartTest(interval, maxLoops);
        }

        private void StartTest(float interval, int maxLoops)
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
            m_loopsCompleted = 0;
            Jotunn.Logger.LogWarning($"Starting redeem stress test every {interval}s with {m_enabledRedeemTitles.Count} enabled redeems, stopping after {maxLoops} loop(s) through the list. Run 'testWBTI' with no args to stop early.");
            m_testCoroutine = WizshBoneTwitchIntegration.Instance.StartCoroutine(TestLoop(interval, maxLoops));
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

        private IEnumerator TestLoop(float interval, int maxLoops)
        {
            ConsoleCommand command = CommandManager.Instance.CustomCommands.FirstOrDefault(item => item.Name == "WBTIUseRedeem");

            if (command == null)
            {
                Jotunn.Logger.LogError("Could not find WBTIUseRedeem command!");
                m_testCoroutine = null;
                yield break;
            }

            while (m_loopsCompleted < maxLoops)
            {
                string redeem = m_enabledRedeemTitles[m_redeemIndex];
                Jotunn.Logger.LogWarning($"[TestWBTI] Firing redeem ({m_redeemIndex + 1}/{m_enabledRedeemTitles.Count}): {redeem}");
                command.Run(new string[] { redeem });

                m_redeemIndex++;

                // A full loop through the list just completed whenever the index wraps back to 0.
                if (m_redeemIndex >= m_enabledRedeemTitles.Count)
                {
                    m_redeemIndex = 0;
                    m_loopsCompleted++;
                }

                if (m_loopsCompleted >= maxLoops)
                    break;

                yield return new WaitForSeconds(interval);
            }

            Jotunn.Logger.LogWarning($"[TestWBTI] Stress test finished after {m_loopsCompleted} loop(s) through the enabled redeems.");
            m_testCoroutine = null;
        }
    }
}
