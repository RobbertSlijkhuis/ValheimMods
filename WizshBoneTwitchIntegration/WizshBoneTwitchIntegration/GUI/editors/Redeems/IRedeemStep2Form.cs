using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// One redeem type's step-2 ("configure parameters") form. <see cref="RedeemWizard"/> builds
    /// one sub-root GameObject + one instance per real type up front (mirroring how
    /// m_step1Root/m_step2Root/m_step3Root are already built once and toggled via SetActive), and
    /// dispatches to whichever form matches <c>m_working.type</c>.
    /// </summary>
    internal interface IRedeemStep2Form
    {
        /// <summary>Builds this type's widgets once as children of <paramref name="parent"/> (a sub-root RedeemWizard creates and toggles active).</summary>
        void Build(GameObject parent);

        /// <summary>
        /// Re-reads every field from <paramref name="working"/>'s relevant sub-data field - called
        /// whenever the wizard (re-)enters step 2 for this type (initial Create-flow selection,
        /// switching type in step 1, Back-into-step-2, or OpenEdit's direct step-2 entry). Safe to
        /// call repeatedly since every field write-on-changes directly into <paramref name="working"/>.
        /// </summary>
        void Populate(RedeemData working);
    }

    /// <summary>
    /// Implemented by a form whose type forces certain sub-data values regardless of what's shown
    /// (e.g. Windmill/Smite/Trap's locked prefab, TerrainEdit's always-on `raise`). Called once
    /// from <see cref="RedeemWizard.Finish"/> right before persisting, so the saved data (and
    /// anything that inspects it, like the redeem list or a hand-exported profile.yaml) reflects
    /// the true forced values immediately - redundant with, but not a replacement for, the
    /// redemption-time normalizer path in <c>helpers/SpawnAbilityHelper.cs</c>'s Normalizers,
    /// which independently re-enforces the same values for hand-edited YAML.
    /// </summary>
    internal interface IForcesValuesOnSave
    {
        void ApplyForcedValues(RedeemData working);
    }

    /// <summary>
    /// Implemented by a form whose type wants non-class-default starting values for a fresh
    /// selection (e.g. Door's 100 doors / 60s duration). Called by <see cref="RedeemWizard"/> only
    /// when the user picks this type in step 1 from a different one - never when re-entering step 2
    /// or opening an existing redeem - so a saved redeem's own values are never overwritten. Needed
    /// for fields that can't be defaulted in <see cref="IRedeemStep2Form.Populate"/> because a
    /// non-nullable value (e.g. <c>duration</c>) can't tell "unset" from an intentional 0.
    /// </summary>
    internal interface IAppliesDefaultsOnSelect
    {
        void ApplyDefaults(RedeemData working);
    }
}
