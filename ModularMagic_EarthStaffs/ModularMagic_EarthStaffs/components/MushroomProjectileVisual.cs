using UnityEngine;

namespace ModularMagic_EarthStaffs.Components
{
    /// <summary>
    /// Picks the random thrown item of the mushroom projectile. The owner rolls it once and stores it in the ZDO,
    /// every client (including the owner) shows the visual that is stored there, so all players see the same item.
    /// </summary>
    internal class MushroomProjectileVisual : MonoBehaviour
    {
        private const string ZdoKey = "MushroomVisual_MMES";

        private static readonly string[] visualNames =
        {
            "Mushroom", "MushroomBlue", "MushroomYellow", "Branch", "Dandelion", "Stone", "Flint", "RaspberryBush"
        };

        // Start instead of Awake, the ZDO is created in the Awake of the ZNetView, which can run after this Awake
        public void Start()
        {
            ZNetView netView = GetComponent<ZNetView>();

            if (netView == null || !netView.IsValid())
                return;

            ZDO zdo = netView.GetZDO();
            int index = zdo.GetInt(ZdoKey, -1);

            if (index < 0 && netView.IsOwner())
            {
                index = Random.Range(0, visualNames.Length);
                zdo.Set(ZdoKey, index);
            }

            // Not rolled (yet), show the first one instead of every visual at once
            if (index < 0 || index >= visualNames.Length)
                index = 0;

            ShowVisual(index);
        }

        private void ShowVisual(int index)
        {
            Transform visuals = transform.Find("visual");

            if (visuals == null)
            {
                Jotunn.Logger.LogWarning("[MMES] Mushroom projectile has no visual child");
                return;
            }

            for (int i = 0; i < visualNames.Length; i++)
            {
                Transform visual = visuals.Find(visualNames[i]);

                if (visual != null)
                    visual.gameObject.SetActive(i == index);
            }
        }
    }
}
