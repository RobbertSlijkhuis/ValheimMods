using System;

namespace WizshBoneTwitchIntegration.Gui
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class OnValueChangedAttribute : Attribute
    {
        public string MethodName { get; }
        public OnValueChangedAttribute(string methodName) { MethodName = methodName; }
    }
}
