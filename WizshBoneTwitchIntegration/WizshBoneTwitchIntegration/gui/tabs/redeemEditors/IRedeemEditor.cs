using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    internal interface IRedeemEditor
    {
        /// <summary>Creates the UI fields inside the given parent container.</summary>
        void BuildUI(UnityEngine.GameObject parent);

        /// <summary>Applies the entered values onto the given RedeemData.</summary>
        void ApplyTo(RedeemData target);

        /// <summary>Resets all fields to their defaults.</summary>
        void Reset();
    }
}