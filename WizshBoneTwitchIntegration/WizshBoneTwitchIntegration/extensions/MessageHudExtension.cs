namespace WizshBoneTwitchIntegration.Extensions
{
    internal static class MessageHudExtension
    {
        public static bool HideCenterMessage(this MessageHud messageHud)
        {
            messageHud.m_messageCenterText.CrossFadeAlpha(0f, 0f, ignoreTimeScale: true);
            messageHud._crossFadeTextBuffer.Clear();
            return true;
        }
    }
}
