namespace WizshBoneTwitchIntegration.Commands
{
    /// <summary>
    /// Shared pass/fail counter + logging for the "WBTITest*" in-game self-test console commands
    /// (see TestProfilesCommand, TestRedeemsCommand), so each one doesn't reimplement the same
    /// bookkeeping.
    /// </summary>
    internal class SelfTestReport
    {
        private readonly string m_tag;

        public int Passed { get; private set; }
        public int Failed { get; private set; }

        public SelfTestReport(string tag)
        {
            m_tag = tag;
        }

        public void Check(bool condition, string testName)
        {
            if (condition)
            {
                Passed++;
                Jotunn.Logger.LogWarning($"[{m_tag}][PASS] {testName}");
            }
            else
            {
                Failed++;
                Jotunn.Logger.LogError($"[{m_tag}][FAIL] {testName}");
            }
        }

        /// <summary>
        /// Banner logged first thing in a run, so the output of self-tests run back to back
        /// doesn't blur together.
        /// </summary>
        public void LogHeader(string subject)
        {
            Jotunn.Logger.LogWarning($"[{m_tag}] ================ {subject} self-test: START ================");
        }

        public void LogSummary(string subject)
        {
            Jotunn.Logger.LogWarning($"[{m_tag}] {subject} self-test complete: {Passed} passed, {Failed} failed.");
            Jotunn.Logger.LogWarning($"[{m_tag}] ================ {subject} self-test: END ================");
        }
    }
}
